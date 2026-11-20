using System;
using System.Collections;
using UnityEngine;


public enum PihuchenState
{
    MoveAround,
    WindGustTelegraph,
    AirGustAttack,
    FireBeamAttack,
    Vulnerable
}

public class Pihuchen : MonoBehaviour, IDamageable
{

    public event Action OnDefeated;
    [Header("Vida del Cuerpo")]
    [SerializeField] private float maxBodyHealth = 800f;
    private float currentBodyHealth;

    [Header("Configuración de Movimiento y Combate")]

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("Capas que bloquea el rayo. Excluye la capa del propio enemigo.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Ajustes de Distancia de Combate")]
    [Tooltip("Distancia mínima que el enemigo intenta mantener respecto al jugador.")]
    [SerializeField] private float minCombatDistance = 8f;
    [Tooltip("Distancia máxima ideal a la que se acerca para atacar.")]
    [SerializeField] private float preferredCombatDistance = 12f;

    [Header("Corrección de Rotación del Modelo")]
    [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;

    [Header("Forzar Ataque por Inactividad del Player")]
    [SerializeField] private float maxTimeWithoutApproach = 6f;

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] airMuzzles;
    [SerializeField] private Transform fireMuzzle;

    [Header("Rayo de Fuego")]
    [SerializeField] private LineRenderer fireBeamLine;
    [SerializeField] private float fireBeamDuration = 3f;
    [Tooltip("Punto del jugador al que apunta el rayo (desde su origen/pies).")]
    [SerializeField] private float aimHeightOffset = 1f;
    [SerializeField] private float fireBeamRangeMultiplier = 1.5f;
    [Tooltip("Alcance máximo al que el rayo se extiende para llegar al jugador (ataques forzados por inactividad).")]
    [SerializeField] private float fireBeamMaxRange = 300f;
    [Tooltip("Dibuja en la Scene view la línea muzzle -> jugador y escribe un log al disparar.")]
    [SerializeField] private bool debugBeam = true;

    [Header("Ráfaga de Viento")]
    [SerializeField] private GameObject airGustPrefab;
    [SerializeField] private float windTelegraphDuration = 2f;

    [Header("Ciclo de Ataques")]
    [SerializeField] private int attacksBeforeMoving = 2;

    [Header("Sistema de Alas y Alturas")]
    [SerializeField] private PihuchenWing leftWing;
    [SerializeField] private PihuchenWing rightWing;
    [SerializeField] private int wingMaxHealth = 3;
    [SerializeField] private float vulnerableDuration = 5f;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float groundYCoord = 0f;
    [SerializeField] private float flyYCoord = 5f;

    private PihuchenState currentState = PihuchenState.MoveAround;
    private float attackTimer;
    private float timeAwayFromPlayer;
    private int consecutiveAttacks;
    private bool areWingsDestroyed;

    // Variables para el movimiento errático/aleatorio
    private Vector3 randomMoveDirection;
    private float changeDirectionTimer;

    // Mientras sea true, ninguna otra acción puede empezar ni mover al enemigo.
    private bool isPerformingAction;

    private Coroutine actionRoutine;
    private Coroutine vulnerableRoutine;

    private float BeamRange => attackRange * fireBeamRangeMultiplier;

    int fireCount = 0;
    int airGutsCount = 0;

    #region Unity

    private void Awake()
    {
        currentBodyHealth = maxBodyHealth;
        if (fireBeamLine != null)
        {
            fireBeamLine.useWorldSpace = true;
            fireBeamLine.positionCount = 2;
            fireBeamLine.enabled = false;
        }

        ResetWings();
        GetNewRandomDirection();
    }

    private void Start()
    {
        player = GameManager.Instance.player.transform;
        SyncPlayerTarget("Start");
    }

    private void Update()
    {
    
        if (player == null) return;
        SyncPlayerTarget("Update");
        attackTimer += Time.deltaTime;

        CheckWingsStatus();
        DetermineState();
        ExecuteState();
    }

    #endregion

    #region sync
    [Header("Debug Objetivo")]
[SerializeField] private bool logTargetDebug = true;
private Transform lastLoggedPlayer;

private void SyncPlayerTarget(string reason)
{
    GameObject gmPlayer = GameManager.Instance != null ? GameManager.Instance.player : null;
    Transform targetTransform = gmPlayer != null ? gmPlayer.transform : player;

    if (targetTransform != null && targetTransform != player)
    {
        player = targetTransform;
    }

    if (logTargetDebug && player != lastLoggedPlayer)
    {
        lastLoggedPlayer = player;
        string gmName = gmPlayer != null ? $"{gmPlayer.name} (id:{gmPlayer.GetInstanceID()})" : "null";
        string pName = player != null ? $"{player.name} (id:{player.GetInstanceID()})" : "null";
        Vector3 pPos = player != null ? player.position : Vector3.zero;
        Vector3 pRootPos = player != null ? player.root.position : Vector3.zero;

        Debug.Log(
            $"[Pihuchen TargetSync | {reason}] Boss='{name}' BossPos={transform.position} | " +
            $"Player='{pName}' Pos={pPos} RootPos={pRootPos} | GM.player='{gmName}'",
            this
        );
    }
}
#endregion

    #region Alas

    private void ResetWings()
    {
        if (leftWing != null)
        {
            leftWing.gameObject.SetActive(true);
            leftWing.InitWing(wingMaxHealth);
        }
        if (rightWing != null)
        {
            rightWing.gameObject.SetActive(true);
            rightWing.InitWing(wingMaxHealth);
        }
        areWingsDestroyed = false;
    }

    private void CheckWingsStatus()
    {
        if (areWingsDestroyed) return;

        bool leftDead = leftWing != null && leftWing.IsDestroyed();
        bool rightDead = rightWing != null && rightWing.IsDestroyed();
        if (!(leftDead && rightDead)) return;

        areWingsDestroyed = true;

        CancelCurrentAction();

        if (vulnerableRoutine != null) StopCoroutine(vulnerableRoutine);
        vulnerableRoutine = StartCoroutine(VulnerableRoutine());

        Debug.Log("[Pihuchen] ¡Ambas alas destruidas! Cayendo al suelo.");
    }

    #endregion

    #region Máquina de estados

    private void DetermineState()
    {
        if (areWingsDestroyed)
        {
            currentState = PihuchenState.Vulnerable;
            return;
        }

        if (isPerformingAction) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange) timeAwayFromPlayer += Time.deltaTime;
        else timeAwayFromPlayer = 0f;

        if (consecutiveAttacks >= attacksBeforeMoving)
        {
            currentState = PihuchenState.MoveAround;
            if (attackTimer >= attackCooldown * 1.5f)
            {
                consecutiveAttacks = 0;
                attackTimer = 0f;
            }
            return;
        }

        bool forceAttack = timeAwayFromPlayer >= maxTimeWithoutApproach;
        bool canAttack = (distanceToPlayer <= attackRange || forceAttack) && attackTimer >= attackCooldown;

        if (canAttack)
        {
            StartRandomAttack();
        }
        else
        {
            currentState = PihuchenState.MoveAround;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case PihuchenState.MoveAround:
                MoveAroundScenario();
                break;

            case PihuchenState.Vulnerable:
                FallToGroundAndBeVulnerable();
                break;

            case PihuchenState.WindGustTelegraph:
            case PihuchenState.AirGustAttack:
            case PihuchenState.FireBeamAttack:
                break;
        }
    }

    #endregion

    #region Ataques

    private void StartRandomAttack()
    {
        attackTimer = 0f;
        timeAwayFromPlayer = 0f;
        consecutiveAttacks++;

        if (UnityEngine.Random.value > 0.5f || fireCount >= 3)
        {
            currentState = PihuchenState.WindGustTelegraph;
            actionRoutine = StartCoroutine(WindGustRoutine());
            airGutsCount++;
            fireCount=0;
        }
        else if(UnityEngine.Random.value < 0.5f || airGutsCount >= 3)
        {
            Debug.Log("Fuegooooooooo");
            currentState = PihuchenState.FireBeamAttack;
            actionRoutine = StartCoroutine(FireBeamRoutine());
            fireCount++;
            airGutsCount = 0;
        }
    }

    private void CancelCurrentAction()
    {
        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }

        SetBeamActive(false);
        isPerformingAction = false;
    }

    private void FinishAction()
    {
        isPerformingAction = false;
        actionRoutine = null;
        attackTimer = 0f; 
        currentState = PihuchenState.MoveAround;
    }

    private IEnumerator WindGustRoutine()
    {
        Debug.Log("ataque ala");
        isPerformingAction = true;
        float elapsed = 0f;
        while (elapsed < windTelegraphDuration)
        {
            FacePlayerSmoothly();
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentState = PihuchenState.AirGustAttack;
        FacePlayerSmoothly();
        SpawnAirGusts();

        FinishAction();
    }

    private void SpawnAirGusts()
    {
        if (airMuzzles == null || airGustPrefab == null) return;

        Vector3 gustTarget = player != null ? player.position : Vector3.zero;
        if (logTargetDebug)
        {
            Debug.Log(
                $"[Pihuchen AirGust] BossPos={transform.position} | Player='{player?.name}' " +
                $"PlayerPos={gustTarget} | PlayerRootPos={player?.root.position}",
                this
            );
        }

        Debug.Log("disparando rafaga");
        foreach (Transform muzzle in airMuzzles)
        {
            if (muzzle == null) continue;

            GameObject gust = Instantiate(airGustPrefab, muzzle.position, Quaternion.identity);
            AirGustProjectile projectile = gust.GetComponent<AirGustProjectile>();
            if (projectile != null) projectile.Initialize(gustTarget);
        }
    }

    private IEnumerator FireBeamRoutine()
    {
        isPerformingAction = true;
        SetBeamActive(true);

        bool hasDealtBeamDamage = false;
        Vector3 fixedAimPoint = player.position + Vector3.up * aimHeightOffset;

        if (debugBeam || logTargetDebug)
        {
            Vector3 dbgOrigin = fireMuzzle != null ? fireMuzzle.position : transform.position;
            Debug.Log(
                $"[Pihuchen BeamStart] MuzzlePos={dbgOrigin} | FixedAimPoint={fixedAimPoint} | " +
                $"Player='{player.name}' PlayerPos={player.position} | GM.playerPos={GameManager.Instance?.player?.transform.position}",
                this
            );
        }

        float elapsed = 0f;
        while (elapsed < fireBeamDuration)
        {
            FacePlayerSmoothly();
            UpdateBeam(fixedAimPoint, ref hasDealtBeamDamage);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetBeamActive(false);
        FinishAction();
    }

    private void UpdateBeam(Vector3 aimPoint, ref bool hasDealtBeamDamage)
    {
        if (fireMuzzle == null || fireBeamLine == null) return;

        Vector3 origin = fireMuzzle.position;
        Vector3 toAim = aimPoint - origin;
        Vector3 direction = toAim.normalized;

        float range = Mathf.Clamp(toAim.magnitude + 2f, BeamRange, Mathf.Max(BeamRange, fireBeamMaxRange));
        Vector3 endPoint = origin + direction * range;

        if (debugBeam)
        {
            Debug.DrawLine(origin, aimPoint, Color.green);
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            if (!hasDealtBeamDamage && (hit.collider.CompareTag("Player") || hit.collider.transform.root.CompareTag("Player")))
            {
                OnBeamHitPlayer(hit, ref hasDealtBeamDamage);
            }
        }

        fireBeamLine.SetPosition(0, origin);
        fireBeamLine.SetPosition(1, endPoint);
    }

    private void OnBeamHitPlayer(RaycastHit hit, ref bool hasDealtBeamDamage)
    {
        bool isPlayerHit = hit.collider.CompareTag("Player") || hit.collider.transform.root.CompareTag("Player");
        if (!isPlayerHit || hasDealtBeamDamage) return;

        PlayerStats playerStats = hit.collider.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(3);
            hasDealtBeamDamage = true;
            Debug.Log($"[Pihuchen] ¡El rayo dañó al jugador ({3})! No volverá a dañar hasta el próximo rayo.");
        }
    }

    private void SetBeamActive(bool active)
    {
       
            if (fireBeamLine != null) fireBeamLine.enabled = active;
        
    }

    #endregion

    #region Movimiento y vulnerabilidad

    private void MoveAroundScenario()
{
    FacePlayerSmoothly();

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    Vector3 moveDirection = Vector3.zero;

    if (distanceToPlayer < minCombatDistance)
    {
        moveDirection = (transform.position - player.position).normalized;
    }
    else if (distanceToPlayer > preferredCombatDistance)
    {
        moveDirection = (player.position - transform.position).normalized;
    }
    else
    {
        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0f)
        {
            GetNewRandomDirection();
        }
        moveDirection = randomMoveDirection;
    }

    moveDirection.y = 0f; // Mantenemos el movimiento plano en XZ
    
    // Opcional: O podrías ajustar sutilmente la posición Y si hay desniveles, 
    // pero manteniendo la base de vuelo limpia.
    transform.position += moveDirection * moveSpeed * Time.deltaTime;
}

    private void GetNewRandomDirection()
    {
        // Genera una dirección aleatoria en el plano XZ
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized;
        randomMoveDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
        changeDirectionTimer = UnityEngine.Random.Range(3f, 6f); // Cambia de rumbo cada 3 a 6 segundos
    }

   private void FallToGroundAndBeVulnerable()
{
    // Lanzamos un rayo hacia abajo desde la posición actual para encontrar la altura exacta del suelo
    float targetY = groundYCoord;
    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, hitMask))
    {
        targetY = hit.point.y + 0.5f; // Ajusta el 0.5f según el tamaño o el pivote de tu modelo
    }

    Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);
    transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);
}

    private IEnumerator VulnerableRoutine()
{
    yield return new WaitForSeconds(vulnerableDuration);

    Debug.Log("[Pihuchen] Tiempo de vulnerabilidad terminado. ¡Las alas se han regenerado, se levanta y vuelve a volar!");
    ResetWings();

    // CORRECCIÓN: Calculamos la altura de vuelo sumando flyYCoord por encima del suelo actual
    float targetY = groundYCoord + flyYCoord;
    if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 100f, hitMask))
    {
        targetY = hit.point.y + flyYCoord;
    }

    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

    consecutiveAttacks = 0;
    attackTimer = 0f;
    vulnerableRoutine = null;
    currentState = PihuchenState.MoveAround;
}

    private void FacePlayerSmoothly()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized) * Quaternion.Euler(modelRotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!debugBeam || player == null) return;

        Gizmos.color = Color.blue;   
        Gizmos.DrawRay(transform.position, transform.forward * 10f);

        Gizmos.color = Color.yellow; 
        Gizmos.DrawLine(transform.position, player.position);
    }

    #endregion

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        
        if (!areWingsDestroyed) return; 

        currentBodyHealth -= amount;
        Debug.Log($"[Pihuchen] El cuerpo recibió {amount} de daño. Vida restante: {currentBodyHealth}");

        if (currentBodyHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Pihuchen] ¡El jefe ha sido derrotado!");
        OnDefeated?.Invoke();
        Destroy(gameObject);
    }
}