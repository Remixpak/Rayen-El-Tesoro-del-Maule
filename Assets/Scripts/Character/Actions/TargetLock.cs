using UnityEngine;

// este script se encarga del sistema de fijar enemigos (lock on): busca al enemigo mas centrado
// en la camara, lo fija, muestra un marcador encima y permite cambiar entre varios enemigos
// moviendo el raton o el stick
[RequireComponent(typeof(PlayerInputHandler))]
public class TargetLock : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private string enemyTag = "Enemy"; // tag que deben tener los enemigos para poder fijarlos
    [SerializeField] private string npcTag = "NPC";
    [SerializeField] private bool canTargetNpcs = true; //permite fijar a los npc
    [SerializeField] private float lockRange = 15f; // distancia maxima para fijar
    [SerializeField] private float breakRange = 20f; // distancia a la que se suelta solo
    [SerializeField, Range(10f, 180f)] private float maxViewAngle = 70f; // angulo maximo respecto a la camara
    [SerializeField] private LayerMask obstacleMask; // capas que tapan la vision (entorno). Vacio = no comprueba

    [Header("Cambio de objetivo")]
    [SerializeField] private bool allowSwitching = true; // si se puede cambiar de enemigo una vez fijado
    [SerializeField] private float mouseSwitchTreshold = 60f; // movimiento acumulado de mouse necesario para cambiar
    [SerializeField] private float stickSwitchTreshold = 0.7f; // cuanto hay que empujar el stick para cambiar
    [SerializeField] private float stickResetTreshhold = 0.3f; // el stick debe bajar de esto para permitir otro cambio
    [SerializeField] private float switchCooldown = 0.3f; // tiempo minimo entre cambios
    [SerializeField, Range(10f, 90f)] private float switchConeAngle = 60f; // tolerancia de direccion en pantalla

    [Header("Marcador (opcional)")]
    [SerializeField] private Transform marker; // objeto que se coloca sobre el enemigo fijado (lo reemplzamos luego con el del felix o la wea que haga el nacho)
    [SerializeField] private float markerHeightOffset = 0.5f; // cuanto mas arriba del enemigo va el marcador

    private PlayerInputHandler inputs; // referencia al input del jugador
    private Transform cameraTransform; // transform de la camara principal
    private Collider currentTarget; // collider del enemigo fijado (null si no hay ninguno)

    private Camera mainCam; // referencia a la camara principal (para WorldToScreenPoint)
    private Vector2 mouseAccum; // acumulador del movimiento del raton (para detectar el gesto de cambio)
    private bool stickReleased = true; // para saber si el stick volvio al centro y se puede volver a cambiar
    private float lastSwitchTime; // momento del ultimo cambio (para el cooldown)

    private readonly Collider[] buffer = new Collider[32]; // buffer reutilizable para no generar basura

    // devuelve true si hay un enemigo fijado
    public bool IsLocked => currentTarget != null;

    public Collider CurrentTarget
    {
        get { return currentTarget; }
    }

    // devuelve el transform del enemigo fijado (o null si no hay ninguno)
    public Transform CurrentTargetTransform => currentTarget != null ? currentTarget.transform : null;

    // punto al que apuntan la camara y el personaje (centro del collider del enemigo)
    public Vector3 AimPoint => currentTarget != null
        ? currentTarget.bounds.center
        : transform.position + transform.forward;

    private void Awake()
    {
        inputs = GetComponent<PlayerInputHandler>();

        // si no hay camara principal, avisamos y desactivamos el script
        if (Camera.main == null)
        {
            Debug.LogError("No hay ninguna cámara con el tag MainCamera.", this);
            enabled = false;
            return;
        }
        cameraTransform = Camera.main.transform;
        mainCam = Camera.main;

        // el marcador empieza oculto
        if (marker != null)
        {
            marker.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // alterna entre fijar y soltar
        if (inputs.LockOn)
        {
            inputs.LockOn = false; // consumimos el input

            // si ya habia un enemigo fijado lo soltamos, si no intentamos fijar uno nuevo
            if (IsLocked)
            {
                Release();
            }
            else
            {
                TryAcquire();
            }

            // reseteamos los acumuladores de cambio para no cambiar justo al fijar
            ResetSwitchInput();
        }

        // mientras haya objetivo se valida y permite cambiarlo
        if (IsLocked)
        {
            ValidateTarget(); // comprobamos que sigue siendo valido

            // si sigue habiendo objetivo y se permite cambiar, miramos si el jugador quiere cambiar
            if (IsLocked && allowSwitching)
            {
                HandleSwitchInput();
            }
        }
    }

    private void LateUpdate()
    {
        // si no hay marcador, no hacemos nada
        if (marker == null)
        {
            return;
        }

        // mostramos u ocultamos el marcador segun haya objetivo fijado
        marker.gameObject.SetActive(IsLocked);
        if (!IsLocked)
        {
            return;
        }

        // colocamos el marcador encima del enemigo y lo orientamos a la camara
        Bounds b = currentTarget.bounds;
        marker.position = b.center + Vector3.up * (b.extents.y + markerHeightOffset);
        marker.rotation = cameraTransform.rotation; // siempre mirando a la camara
    }

    // funcion para buscar el enemigo mas cercano y centrado en la camara
    private void TryAcquire()
    {
        // buscamos todos los colliders en un radio de lockRange y los filtramos por tag, angulo y linea de vision
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, lockRange, buffer,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);

        Collider best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];

            // si no tiene el tag de enemigo, lo saltamos
            bool isEnemy = col.CompareTag(enemyTag);
            bool isNpc = canTargetNpcs && col.CompareTag(npcTag);

            if (!isEnemy && !isNpc)
            {
                continue;
            }

            // si esta fuera del angulo de vision, lo saltamos
            Vector3 toEnemy = col.bounds.center - cameraTransform.position;
            float angle = Vector3.Angle(cameraTransform.forward, toEnemy);
            if (angle > maxViewAngle)
            {
                continue;
            }

            // si no hay linea de vision, lo saltamos
            if (!HasLineOfSight(col))
            {
                continue;
            }

            // puntuacion: cuanto mas centrado y mas cerca, mejor (menor es mejor)
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

    // funcion que comprueba si hay linea de vision entre el jugador y el objetivo
    private bool HasLineOfSight(Collider target)
    {
        // si no hay capas de obstaculos, consideramos que siempre hay linea de vision
        if (obstacleMask.value == 0)
        {
            return true;
        }

        Vector3 origin = transform.position + Vector3.up * 1.5f; // lanzamos desde la altura del pecho
        bool blocked = Physics.Linecast(
            origin, target.bounds.center, obstacleMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    // funcion para validar si el objetivo fijado sigue siendo valido, si no lo es se suelta
    private void ValidateTarget()
    {
        // si el collider se ha desactivado, el enemigo ha muerto o se ha alejado demasiado, lo soltamos
        if (!currentTarget.enabled || !currentTarget.gameObject.activeInHierarchy ||
            Vector3.Distance(transform.position, currentTarget.bounds.center) > breakRange)
        {
            Release();
        }
    }

    // suelta el enemigo fijado y limpia los acumuladores
    public void Release()
    {
        currentTarget = null;
        mouseAccum = Vector2.zero;
    }

    // dibuja el rango y la distancia de ruptura en el editor (solo para depurar)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, breakRange);
    }

    // resetea los acumuladores de cambio justo despues de fijar o soltar un enemigo
    private void ResetSwitchInput()
    {
        mouseAccum = Vector2.zero;
        stickReleased = false; // exige soltar el stick antes del primer cambio
        lastSwitchTime = Time.time;
    }

    // metodo para mirar si el jugador esta intentando cambiar de enemigo con raton o stick
    private void HandleSwitchInput()
    {
        Vector2 look = inputs.look; // input de la camara (raton o stick)
        Vector2 dir = Vector2.zero; // direccion del cambio (0 = no cambiar)

        if (inputs.LookIsGamepad)
        {
            // con mando: el stick debe volver al centro antes de poder cambiar otra vez
            float mag = look.magnitude;
            if (mag < stickResetTreshhold)
            {
                stickReleased = true;
            }

            if (stickReleased && mag >= stickSwitchTreshold)
            {
                dir = look / mag; // direccion normalizada del stick
                stickReleased = false;
            }
        }
        else
        {
            // con raton: acumulamos el movimiento y se va "gastando" con el tiempo
            mouseAccum += look;
            mouseAccum = Vector2.Lerp(mouseAccum, Vector2.zero, 8f * Time.unscaledDeltaTime);

            if (mouseAccum.magnitude >= mouseSwitchTreshold)
            {
                dir = mouseAccum.normalized;
                mouseAccum = Vector2.zero;
            }
        }

        // si no hay direccion, no hacemos nada
        if (dir == Vector2.zero)
        {
            return;
        }

        // si aun no ha pasado el cooldown, no cambiamos
        if (Time.time - lastSwitchTime < switchCooldown)
        {
            return;
        }

        // si el cambio se ha hecho bien, actualizamos el tiempo del ultimo cambio
        if (TrySwitch(dir))
        {
            lastSwitchTime = Time.time;
        }
    }

    // metodo para cambiar al enemigo que este en la direccion pedida (en pantalla)
    private bool TrySwitch(Vector2 dir)
    {
        // posicion en pantalla del enemigo actual (para comparar distancias)
        Vector3 currentScreen = mainCam.WorldToScreenPoint(currentTarget.bounds.center);

        // buscamos todos los enemigos dentro del rango
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, lockRange, buffer,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);

        Collider best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];

            // si es el actual o no es enemigo, lo saltamos
            if (col == currentTarget || !col.CompareTag(enemyTag))
            {
                continue;
            }

            Vector3 screen = mainCam.WorldToScreenPoint(col.bounds.center);
            if (screen.z <= 0f)
            {
                continue; // esta detras de la camara
            }
            if (!HasLineOfSight(col))
            {
                continue; // no tenemos linea de vision
            }

            Vector2 delta = (Vector2)screen - (Vector2)currentScreen;
            if (delta.sqrMagnitude < 1f)
            {
                continue; // practicamente en el mismo punto
            }

            float angle = Vector2.Angle(dir, delta);
            if (angle > switchConeAngle)
            {
                continue; // no esta en la direccion pedida
            }

            // menor es mejor: priorizamos alineacion y cercania en pantalla
            float score = (angle / switchConeAngle) + (delta.magnitude / Screen.height);
            if (score < bestScore)
            {
                bestScore = score;
                best = col;
            }
        }

        // si no hay nadie en esa direccion, nos quedamos con el actual
        if (best == null)
        {
            return false;
        }

        currentTarget = best; // cambiamos al nuevo enemigo
        return true;
    }
}