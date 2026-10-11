using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class TargetLock : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float lockRange = 15f;      // distancia máxima para fijar
    [SerializeField] private float breakRange = 20f;     // distancia a la que se suelta solo
    [SerializeField, Range(10f, 180f)] private float maxViewAngle = 70f; // ángulo máximo respecto a la cámara
    [SerializeField] private LayerMask obstacleMask;     // capas que tapan la visión (entorno). Vacío = no comprueba

    [Header("Marcador (opcional)")]
    [SerializeField] private Transform marker;           // objeto que se coloca sobre el enemigo fijado (lo reemplzamos luego con el del felix o la wea que haga el nacho)
    [SerializeField] private float markerHeightOffset = 0.5f;

    private PlayerInputHandler inputs;
    private Transform cameraTransform;
    private Collider currentTarget;
    private readonly Collider[] buffer = new Collider[32];

    public bool IsLocked => currentTarget != null;

    // Punto al que apuntan la cámara y el personaje (centro del collider del enemigo)
    public Vector3 AimPoint => currentTarget != null
        ? currentTarget.bounds.center
        : transform.position + transform.forward;

    private void Awake()
    {
        inputs = GetComponent<PlayerInputHandler>();

        if (Camera.main == null)
        {
            Debug.LogError("No hay ninguna cámara con el tag MainCamera.", this);
            enabled = false;
            return;
        }
        cameraTransform = Camera.main.transform;

        if (marker != null) marker.gameObject.SetActive(false);
    }

    private void Update()
    {
        // si se pulsa el botón de lock on, se alterna entre fijar y soltar
        if (inputs.LockOn)
        {
            inputs.LockOn = false;
            if (IsLocked) Release();
            else TryAcquire();
        }

        // si hay un objetivo fijado se valida que siga siendo valido
        if (IsLocked) ValidateTarget();
    }

    private void LateUpdate()
    {
        //si hay un objetivo fijado, se actualiza la posición del marcador sobre el enemigo
        if (marker == null) return;

        marker.gameObject.SetActive(IsLocked);
        if (!IsLocked) return;

        Bounds b = currentTarget.bounds;
        marker.position = b.center + Vector3.up * (b.extents.y + markerHeightOffset);
        marker.rotation = cameraTransform.rotation; // siempre mirando a la cámara
    }

    //funcion para buscar el enemigo más cercano y centrado en la cámara, si lo encuentra se fija sobre él, si no lo encuentra se queda sin objetivo fijado
    private void TryAcquire()
    {
        // buscamos todos los colliders en un radio de lockRange y los filtramos por tag, ángulo y línea de visión
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, lockRange, buffer,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);

        Collider best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];
            if (!col.CompareTag(enemyTag)) continue;

            Vector3 toEnemy = col.bounds.center - cameraTransform.position;
            float angle = Vector3.Angle(cameraTransform.forward, toEnemy);
            if (angle > maxViewAngle) continue;

            if (!HasLineOfSight(col)) continue;

            // Puntuación: cuanto más centrado y más cerca, mejor (menor es mejor)
            float dist = Vector3.Distance(transform.position, col.bounds.center);
            float score = angle / maxViewAngle + dist / lockRange;

            if (score < bestScore)
            {
                bestScore = score;
                best = col;
            }
        }

        currentTarget = best; // si no hay ninguno queda en null y no se fija nada
    }

    //funcion que comprueba si hay linea de vision entre el jugador y el objetivo fijado, si no hay linea de vision se suelta el objetivo
    private bool HasLineOfSight(Collider target)
    {
        if (obstacleMask.value == 0) return true;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        bool blocked = Physics.Linecast(
            origin, target.bounds.center, obstacleMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    //funcion para validar si el objetivo fijado sigue siendo valido, si no lo es se suelta
    private void ValidateTarget()
    {
        if (!currentTarget.enabled || !currentTarget.gameObject.activeInHierarchy ||
            Vector3.Distance(transform.position, currentTarget.bounds.center) > breakRange)
        {
            Release();
        }
    }

    public void Release() => currentTarget = null;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, breakRange);
    }
}