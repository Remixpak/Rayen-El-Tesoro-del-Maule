using UnityEngine;

// compañero que flota junto al jugador y se ilumina cuando hay un cofre sin abrir cerca
//basicamente el script se encarga de hacer funcionar al claurunco, el cual vuela alrededor del jugador y brilal de forma intermitente al sentir un cofre sin abrir cerca

//recordar meterlo dentro del prefab padre del jugador(donde esta el pjRayen,la camara y otras weas) y hacerlo un prefab pls 
public class ClaurunkoCompanion : MonoBehaviour
{
    [Header("Seguimiento")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 followOffset = new Vector3(0.9f, 1.6f, -0.2f); // relativo al jugador (derecha, arriba, atras) (posicion del claurunco)
    [SerializeField] private float followSmoothTime = 0.25f;
    [SerializeField] private float bobAmplitude = 0.08f; // cuanto sube y baja
    [SerializeField] private float bobSpeed = 2f;

    [Header("Detección de cofres")]
    [SerializeField] private string chestTag = "Chest";
    [SerializeField] private float detectRadius = 12f;
    [SerializeField] private float scanInterval = 0.25f; // cada cuanto busca (no hace falta cada frame)
    [SerializeField] private LayerMask scanMask = ~0;

    [Header("Brillo")]
    [SerializeField] private Renderer cubeRenderer;
    [SerializeField] private Light glowLight; // opcional
    [SerializeField] private Color idleColor = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private float maxLightIntensity = 2.5f;
    [SerializeField] private float pulseSpeedFar = 2f;   // pulso lento si el cofre esta lejos
    [SerializeField] private float pulseSpeedNear = 8f;  // pulso rapido si esta cerca
    [SerializeField] private float glowFadeSpeed = 5f;

    private readonly Collider[] buffer = new Collider[32];
    private Material material;
    private Vector3 velocity;
    private float nextScanTime;
    private float proximity;       // 0 = no hay cofre cerca, 1 = muy cerca
    private float currentGlow;     // brillo actual suavizado

    // true mientras hay un cofre sin abrir dentro del radio (por si otros sistemas lo quieren usar)
    public bool ChestNearby { get; private set; }

    private void Awake()
    {
        if (cubeRenderer == null) cubeRenderer = GetComponentInChildren<Renderer>();
        if (cubeRenderer != null) material = cubeRenderer.material; // crea una copia propia

        if (player == null)
        {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null) player = movement.transform;
        }

        if (player == null)
        {
            Debug.LogError("ClaurunkoCompanion: no se encontro al jugador.", this);
            enabled = false;
            return;
        }

        transform.position = TargetPosition();
        ApplyGlow(0f, 0f);
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material); // limpia la copia del material
    }

    private void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            Scan();
        }

        UpdateGlow();
    }

    private void LateUpdate()
    {
        // el seguimiento va en LateUpdate para moverse despues del jugador y no temblar
        transform.position = Vector3.SmoothDamp(
            transform.position, TargetPosition(), ref velocity, followSmoothTime);
    }

    private Vector3 TargetPosition()
    {
        Vector3 target = player.TransformPoint(followOffset);
        target.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        return target;
    }

    // busca el cofre sin abrir mas cercano dentro del radio
    private void Scan()
    {
        int count = Physics.OverlapSphereNonAlloc(
            player.position, detectRadius, buffer, scanMask, QueryTriggerInteraction.Ignore);

        float closestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];
            if (!col.CompareTag(chestTag)) continue;

            Chest chest = col.GetComponentInParent<Chest>();
            if (chest == null || !chest.CanOpen) continue; // ya abierto: no avisa

            float sqr = (col.transform.position - player.position).sqrMagnitude;
            if (sqr < closestSqr) closestSqr = sqr;
        }

        ChestNearby = closestSqr < float.MaxValue;

        // 0 en el borde del radio, 1 pegado al cofre
        proximity = ChestNearby
            ? 1f - Mathf.Clamp01(Mathf.Sqrt(closestSqr) / detectRadius)
            : 0f;
    }

    private void UpdateGlow()
    {
        // suaviza la entrada y salida del brillo
        currentGlow = Mathf.MoveTowards(currentGlow, proximity, glowFadeSpeed * Time.deltaTime);

        // pulso: mas rapido cuanto mas cerca
        float pulseSpeed = Mathf.Lerp(pulseSpeedFar, pulseSpeedNear, currentGlow);
        float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * pulseSpeed);

        ApplyGlow(currentGlow, pulse);
    }

    private void ApplyGlow(float glow, float pulse)
    {
        // con glow = 0 se queda con el color apagado
        float strength = glow > 0.001f ? Mathf.Lerp(0.35f, 1f, glow) * pulse : 0f;

        if (material != null)
        {
            material.color = Color.Lerp(idleColor, glowColor, strength);
        }

        if (glowLight != null)
        {
            glowLight.color = glowColor;
            glowLight.intensity = strength * maxLightIntensity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(player.position, detectRadius);
    }
}