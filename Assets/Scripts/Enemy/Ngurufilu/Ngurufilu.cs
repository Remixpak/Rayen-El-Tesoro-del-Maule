using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NgurufiluState
{
    MoveAround,
    DirectAtack,
    TailAtack,
    Stuned
}

public class Ngurufilu : MonoBehaviour, IDamageable
{
    [Header("State & References")]
    [SerializeField] private NgurufiluState currentState;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject tailColliderPrefab; // Prefab o GameObject para instanciar en TailAtack

    [Header("Stats")]
    [SerializeField] private int maxHealth = 800;
    private int currentHealth;
    [SerializeField] private float speed = 20f;

    [Header("Movement Points")]
    [SerializeField] private List<GameObject> moveTo = new List<GameObject>();

    // Contadores para los ataques
    private int consecutiveDirect = 0;
    private int consecutiveTail = 0;
    private int totalAttacksCount = 0;

    private Coroutine activeStateCoroutine;
    private float originalY;

    private void Start()
    {
        currentHealth = maxHealth;
        originalY = transform.position.y;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        // Iniciar en el primer estado
        ChangeState(NgurufiluState.MoveAround);
    }

    public void Update()
    {
        // La lógica de ejecución se maneja mediante corrutinas por estado para mayor orden y control de tiempos.
    }

    private void ChangeState(NgurufiluState newState)
    {
        currentState = newState;

        if (activeStateCoroutine != null)
        {
            StopCoroutine(activeStateCoroutine);
        }

        switch (currentState)
        {
            case NgurufiluState.MoveAround:
                activeStateCoroutine = StartCoroutine(MoveAroundRoutine());
                break;
            case NgurufiluState.DirectAtack:
                activeStateCoroutine = StartCoroutine(DirectAtackRoutine());
                break;
            case NgurufiluState.TailAtack:
                activeStateCoroutine = StartCoroutine(TailAtackRoutine());
                break;
            case NgurufiluState.Stuned:
                activeStateCoroutine = StartCoroutine(StunedRoutine());
                break;
        }
    }

    #region Rutinas de Estados

    // 1. MOVE AROUND: Elige 3 puntos aleatorios, baja, se mueve y vuelve a subir.
    private IEnumerator MoveAroundRoutine()
    {
        if (moveTo == null || moveTo.Count == 0) yield break;

        // Seleccionar 3 puntos aleatorios de la lista
        List<GameObject> selectedPoints = GetRandomPoints(3);

        foreach (GameObject targetPoint in selectedPoints)
        {
            if (targetPoint == null) continue;

            // Bajar (bajar altura Y)
            yield return MoveToHeight(originalY - 2);

            // Moverse horizontalmente hacia el punto
            Vector3 targetPos = new Vector3(targetPoint.transform.position.x, transform.position.y, targetPoint.transform.position.z);
            while (Vector3.Distance(transform.position, targetPos) > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }

            // Volver a subir a la altura original
            yield return MoveToHeight(originalY);
        }

        // Al terminar los 3 puntos, decide si se vuelve a mover o ataca (50% probabilidad)
        DeterminateNextStateAfterMove();
    }

    // 2. DIRECT ATTACK: Movimiento en parábola hacia el jugador
    private IEnumerator DirectAtackRoutine()
    {
        if (player == null)
        {
            ChangeState(NgurufiluState.MoveAround);
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position;
        float duration = 1.5f;
        float elapsed = 0f;
        float parabolaHeight = 4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Interpolación lineal base
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            // Añadir curva de parábola en Y
            currentPos.y += Mathf.Sin(t * Mathf.PI) * parabolaHeight;

            transform.position = currentPos;
            yield return null;
        }

        // Registrar ataque y comprobar si pasa a Stuned o continúa
        RegisterAttackResult(NgurufiluState.DirectAtack);
    }

    // 3. TAIL ATTACK: Elige punto más cercano al player, sube un poco e instancia box collider
    private IEnumerator TailAtackRoutine()
    {
        if (moveTo == null || moveTo.Count == 0 || player == null)
        {
            ChangeState(NgurufiluState.MoveAround);
            yield break;
        }

        // Encontrar el punto de moveTo más cercano al jugador
        GameObject closestPoint = moveTo[0];
        float minDistance = Vector3.Distance(player.position, closestPoint.transform.position);

        foreach (GameObject pt in moveTo)
        {
            float dist = Vector3.Distance(player.position, pt.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint = pt;
            }
        }

        // Moverse al punto cercano
        Vector3 targetPos = new Vector3(closestPoint.transform.position.x, originalY, closestPoint.transform.position.z);
        while (Vector3.Distance(transform.position, targetPos) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        // Subir un poco
        yield return MoveToHeight(originalY + 3f);

        // Instanciar BoxCollider / objeto de ataque
        if (tailColliderPrefab != null)
        {
            Instantiate(tailColliderPrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);
        }

        // Esperar un momento en el aire por el impacto del ataque
        yield return new WaitForSeconds(0.8f);

        // Volver a altura normal
        yield return MoveToHeight(originalY);

        // Registrar ataque
        RegisterAttackResult(NgurufiluState.TailAtack);
    }

    // 4. STUNED: Sube, quieto 3 segundos, vuelve a altura original y pasa a MoveAround
    private IEnumerator StunedRoutine()
    {
        // Subir
        yield return MoveToHeight(originalY + 4f);

        // Quedarse quieto 3 segundos
        yield return new WaitForSeconds(3f);

        // Volver a altura original
        yield return MoveToHeight(originalY);

        // Resetear contadores y volver a MoveAround
        totalAttacksCount = 0;
        consecutiveDirect = 0;
        consecutiveTail = 0;

        ChangeState(NgurufiluState.MoveAround);
    }

    #endregion

    #region Lógica de Transiciones y Pesos

    private void DeterminateNextStateAfterMove()
    {
        // 50% probabilidad de moverse de nuevo o atacar
        bool chooseToAttack = Random.value < 0.3f;

        if (!chooseToAttack)
        {
            ChangeState(NgurufiluState.MoveAround);
        }
        else
        {
            SelectRandomAttack();
        }
    }

    private void SelectRandomAttack()
    {
        NgurufiluState chosenAttack;

        // Regla de peso: Si un ataque se hizo 3 veces seguidas, el siguiente obligatoriamente debe ser el contrario.
        if (consecutiveDirect >= 3)
        {
            chosenAttack = NgurufiluState.TailAtack;
        }
        else if (consecutiveTail >= 3)
        {
            chosenAttack = NgurufiluState.DirectAtack;
        }
        else
        {
            // 50% de probabilidad entre Direct y Tail
            chosenAttack = Random.value < 0.5f ? NgurufiluState.DirectAtack : NgurufiluState.TailAtack;
        }

        ChangeState(chosenAttack);
    }

    private void RegisterAttackResult(NgurufiluState attackType)
    {
        totalAttacksCount++;

        if (attackType == NgurufiluState.DirectAtack)
        {
            consecutiveDirect++;
            consecutiveTail = 0; // Se resetea el contrario
        }
        else if (attackType == NgurufiluState.TailAtack)
        {
            consecutiveTail++;
            consecutiveDirect = 0; // Se resetea el contrario
        }

        // Si ya atacó 3 veces en total, pasa al estado Stuned
        if (totalAttacksCount >= 3)
        {
            ChangeState(NgurufiluState.Stuned);
        }
        else
        {
            // Si no, vuelve a decidir si se mueve o ataca de nuevo
            DeterminateNextStateAfterMove();
        }
    }

    #endregion

    #region Métodos Auxiliares

    private IEnumerator MoveToHeight(float targetY)
    {
        while (Mathf.Abs(transform.position.y - targetY) > 0.05f)
        {
            float newY = Mathf.MoveTowards(transform.position.y, targetY, speed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
    }

    private List<GameObject> GetRandomPoints(int count)
    {
        List<GameObject> copyList = new List<GameObject>(moveTo);
        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < count && copyList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, copyList.Count);
            result.Add(copyList[randomIndex]);
            copyList.RemoveAt(randomIndex); // Evitar repetir el mismo punto en la misma ronda
        }

        return result;
    }

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        currentHealth -= Mathf.RoundToInt(amount);
        Debug.Log("Daño restante: " + currentHealth);
        if (currentHealth <= 0)
        {
            // Lógica de muerte
            Destroy(gameObject);
        }
    }

    #endregion
}