using System.Collections;
using System.Linq;
using UnityEngine;

public enum CuatreroState
{
    Approach,   // Se acerca si el jugador está muy lejos
    Attack,     // Ataca al jugador con el raycast
    Reload,     // Se aleja para recargar si no tiene munición
    Surround,   // Rodea al jugador si el jugador apunta a otro enemigo
    Patrol      // Patrulla por los puntos asignados si no ve al jugador
}

public class Cuatrero : MonoBehaviour
{
    [Header("Configuración de Data")]
    [SerializeField] private CuatreroData cuatreroData;

    [Header("Detección")]
    [SerializeField] private float sightRange = 15f; // Distancia máxima a la que detecta al jugador

    [Header("Referencias")]
    [SerializeField] private Transform player; // Arrastras aquí el GameObject del Jugador
    [SerializeField] private Transform muzzle; // Punta del arma del cuatrero

    [Header("Disparo Visual")]
    [SerializeField] private LineRenderer shootLine;
    [SerializeField] private float shootDuration = 0.05f;

    [Header("Animación (mas adelante)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTrigger = "Shoot";

    [Header("Lista de posiciones para patrullaje")]
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0; // Índice actual del punto de patrulla
    [SerializeField] private float waypointTolerance = 0.5f;

    private CuatreroState currentState;
    private int currentAmmo;
    private bool isReloading = false;
    private float attackTimer = 0f;
    private int triggerHash;
    private Coroutine shotRoutine;

    private TargetLock playerTargetLock; // Referencia al componente de lock del jugador

    private void Awake()
    {
        if (cuatreroData != null)
        {
            currentAmmo = cuatreroData.maxAmmo;
        }

        triggerHash = Animator.StringToHash(shootTrigger);

        if (shootLine != null)
        {
            shootLine.useWorldSpace = true;
            shootLine.positionCount = 2;
            shootLine.enabled = false;
        }

        // Obtener automáticamente el TargetLock del jugador asignado
        if (player != null)
        {
            playerTargetLock = player.GetComponent<TargetLock>();
            if (playerTargetLock == null)
            {
                Debug.LogWarning($"El objeto jugador '{player.name}' no tiene el script TargetLock acoplado.", this);
            }
        }
    }

    private void Update()
    {
        if (player == null || cuatreroData == null) return;

        attackTimer += Time.deltaTime;

        DetermineState();
        ExecuteState();
    }

    private void DetermineState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Verificar si el jugador está a la vista (dentro del radio de visión y con los puntos de patrulla configurados)
        bool playerInSight = distanceToPlayer <= sightRange;

        // 1. Sin munición o recargando -> Huir/Recargar (siempre prioritario si está en combate)
        if (currentAmmo <= 0 || isReloading)
        {
            currentState = CuatreroState.Reload;
        }
        // 2. Si el jugador NO está a la vista Y tenemos puntos de patrulla configurados -> Patrullar
        else if (!playerInSight && patrolPoints != null && patrolPoints.Length > 0)
        {
            currentState = CuatreroState.Patrol;
        }
        else
        {
            // 3. Verificar si el jugador está haciendo Lock a OTRO enemigo distinto de este
            bool playerIsLockedOnSomeoneElse = playerTargetLock != null &&
                                               playerTargetLock.IsLocked &&
                                               playerTargetLock.CurrentTargetTransform != transform;

            // 4. Si el jugador está apuntando a otro enemigo -> Rodear
            if (playerIsLockedOnSomeoneElse)
            {
                currentState = CuatreroState.Surround;
            }
            // 5. Si el jugador está muy lejos (pero dentro del rango de visión) -> Acercarse a hostigar
            else if (distanceToPlayer > cuatreroData.attackRange)
            {
                currentState = CuatreroState.Approach;
            }
            // 6. De lo contrario, atacar
            else
            {
                currentState = CuatreroState.Attack;
            }
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case CuatreroState.Approach:
                ApproachPlayer();
                break;

            case CuatreroState.Attack:
                Attack();
                break;

            case CuatreroState.Reload:
                TakeDistanceAndReload();
                break;

            case CuatreroState.Surround:
                Surround();
                break;

            case CuatreroState.Patrol:
                Patrol();
                break;
        }
    }

    private void ApproachPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

        transform.position += directionToPlayer * cuatreroData.moveSpeed * Time.deltaTime;
    }

    private void Attack()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

        if (attackTimer >= cuatreroData.attackCooldown)
        {
            attackTimer = 0f;
            FireShot();
        }
    }

    private void FireShot()
    {
        if (muzzle == null) return;

        if (animator != null)
        {
            animator.SetTrigger(triggerHash);
        }

        Vector3 origin = muzzle.position;
        Vector3 direction = muzzle.forward;
        float range = cuatreroData.attackRange * 1.5f;
        Vector3 endPoint = origin + direction * range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, cuatreroData.hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<PlayerShooting>() != null)
            {
                Debug.Log("[Cuatrero] ¡Atacó al jugador con éxito!");
            }
        }

        currentAmmo--;
        ShowShootEffect(origin, endPoint);

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void ShowShootEffect(Vector3 start, Vector3 end)
    {
        if (shootLine == null) return;

        shootLine.SetPosition(0, start);
        shootLine.SetPosition(1, end);
        shootLine.enabled = true;

        if (shotRoutine != null) StopCoroutine(shotRoutine);
        shotRoutine = StartCoroutine(HideShootEffect());
    }

    private IEnumerator HideShootEffect()
    {
        yield return new WaitForSeconds(shootDuration);
        shootLine.enabled = false;
    }

    private void Surround()
    {
        // Movimiento lateral alrededor del jugador
        Vector3 directionToPlayer = transform.position - player.position;
        Vector3 sideDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;

        transform.position += sideDirection * cuatreroData.moveSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    private void TakeDistanceAndReload()
    {
        Vector3 runDirection = (transform.position - player.position).normalized;
        runDirection.y = 0;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < cuatreroData.safeDistance)
        {
            transform.position += runDirection * cuatreroData.moveSpeed * Time.deltaTime;
            if (runDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(runDirection);
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null) return;

        Vector3 directionToPoint = (targetPoint.position - transform.position).normalized;
        directionToPoint.y = 0;

        if (directionToPoint != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPoint);
        }

        transform.position += directionToPoint * cuatreroData.moveSpeed * Time.deltaTime;

        // Si llega cerca del punto actual, pasa al siguiente
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                             new Vector3(targetPoint.position.x, 0, targetPoint.position.z)) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

   

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(cuatreroData.reloadTime);
        currentAmmo = cuatreroData.maxAmmo;
        isReloading = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (muzzle != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(muzzle.position, muzzle.forward * (cuatreroData != null ? cuatreroData.attackRange : 10f));
        }

        // Visualizar el rango de visión en el editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}