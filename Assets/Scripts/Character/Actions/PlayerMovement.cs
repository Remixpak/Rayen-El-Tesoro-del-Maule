using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))] // Asegura que el GameObject tenga un CharacterController

//este script se encarga de mover al jugador y controlar la camara, utilizando los inputs recibidos del script PlayerInputHandler (aunque deberia hacerlo mas escalable a futuro para mas botones)
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _char; // referencia al CharacterController del jugador
    private PlayerInputHandler _inputs; // referencia al script PlayerInputHandler que recibe los inputs del jugador
    private Transform cameraTransform; // referencia a la camara principal del juego, para poder mover al jugador en la direccion de la camara
    private TargetLock targetLock; // referencia al script TargetLock que se encarga de fijar enemigos y apuntar hacia ellos

    // Variables de configuracion del jugador y la camara
    [Header("Player")]
    [SerializeField] private float playerSpeed = 5f; // velocidad del jugador
    [SerializeField] private float playerJump = 1.2f; // altura del salto en metros
    [SerializeField] private float turnSmoothTime = 0.1f; // tiempo de suavizado para la rotacion del jugador
    [SerializeField] private float playerGravity = -20f; // gravedad aplicada al jugador
    [SerializeField] private float sprintSpeed = 8f; // velocidad del jugador al sprintar
    [SerializeField] private float sprintAcceleration = 20f; // que tan rapido acelera/frena al cambiar entre andar y sprintar

    [Header("Camara")]
    [SerializeField] private Transform cinemachineCameraTarget; // referencia al gameObject de cameraRoot que es hijo del PJ y que se encarga de mover la camara con Cinemachine
    [SerializeField] private float mouseSensitivity = 0.1f; // sensibilidad del raton para mover la camara
    [SerializeField] private float gamepadSensitivity = 150f; // sensibilidad del gamepad para mover la camara
    [SerializeField] private float bottomClamp = -30f; // es lo maximo que se puede mirar hacia arriba
    [SerializeField] private float topClamp = 60f; // es lo maximo que se puede mirar hacia abajo
    [SerializeField] private float lockCameraSmooth = 8f; // suavizado de la camara al fijar un objetivo

    [Header("Esquive (con lock-on)")]
    [SerializeField] private float dodgeDistance = 3f; // distancia total del paso
    [SerializeField] private float dodgeDuration = 0.35f; // duracion (igualala a la animacion luego)
    [SerializeField] private float dodgeCooldown = 0.3f; // espera antes de poder esquivar otra vez
    [SerializeField] private AnimationCurve dodgeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // para cuando tengamos animacion 
    [SerializeField, Range(0f, 1f)] private float dodgeInvulnerableFraction = 0.6f; // frames de invulnerabilidad 


    private float verticalVelocity; // velocidad vertical del jugador (gravedad y salto)
    private float turnSmoothVelocity; // velocidad de suavizado para la rotacion del jugador
    private float yaw, pitch; // rotacion de la camara en los ejes Y y X
    private float currentSpeed; // velocidad horizontal actual (interpolada para el sprint)

    // Variables para el esquive 
    private bool isDodging;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private Vector3 dodgeDir;

    public bool IsDodging => isDodging;
    public event System.Action<Vector2> DodgeStarted; // direccion local: x = lateral, y = adelante/atras


    private void Awake()
    {
        _char = GetComponent<CharacterController>();
        _inputs = GetComponent<PlayerInputHandler>();
        targetLock = GetComponent<TargetLock>();

        // si no hay ninguna camara con el tag MainCamera, se desactiva el script
        if (Camera.main == null)
        {
            Debug.LogError("No hay ninguna camara con el tag MainCamera.", this);
            enabled = false;
            return;
        }
        cameraTransform = Camera.main.transform;

        currentSpeed = playerSpeed; // inicializamos la velocidad
    }

    private void Start()
    {
        // inicializa la rotacion de la camara si hay cameraTarget
        if (cinemachineCameraTarget != null)
            yaw = cinemachineCameraTarget.eulerAngles.y;

        // bloqueamos el cursor al iniciar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleCursorLock();

        Vector2 move = _inputs.move;
        Vector3 horizontal = Vector3.zero;
        Vector3 dodgeMove = Vector3.zero;
        bool locked = targetLock != null && targetLock.IsLocked;
        float speed = _inputs.sprint ? sprintSpeed : playerSpeed;

        dodgeCooldownTimer -= Time.deltaTime;

        if (isDodging)
        {
            // durante el esquive: ignora el stick, pero sigue mirando al objetivo
            if (locked) FaceTarget();
            dodgeMove = StepDodge();
        }
        else if (locked)
        {
            FaceTarget();

            // movimiento en strafe (relativo a la camara)
            if (move.sqrMagnitude > 0.15f * 0.15f)
            {
                Vector3 input = Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);
                horizontal = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * input * speed;
            }
        }
        else if (move.sqrMagnitude > 0.15f * 0.15f)
        {
            // sin objetivo: movimiento libre (mira hacia donde camina)
            float targetAngle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            horizontal = dir * (speed * Mathf.Clamp01(move.magnitude));
        }

        // gravedad base
        if (_char.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // salto o esquive
        if (_inputs.jump && _char.isGrounded && !isDodging)
        {
            if (locked) TryStartDodge(move); // con lock-on, el salto se convierte en esquive
            else verticalVelocity = Mathf.Sqrt(playerJump * -2f * playerGravity); // sin lock-on, salto normal
        }

        _inputs.jump = false;
        verticalVelocity += playerGravity * Time.deltaTime;

        // aplicamos el movimiento final combinando todo
        _char.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime + dodgeMove);
    }

    private void LateUpdate()
    {
        if (cinemachineCameraTarget == null) return;

        if (targetLock != null && targetLock.IsLocked)
        {
            // con objetivo: la camara se centra sola en el enemigo
            Vector3 dir = targetLock.AimPoint - cinemachineCameraTarget.position;

            if (dir.sqrMagnitude > 0.01f)
            {
                Vector3 e = Quaternion.LookRotation(dir).eulerAngles;
                float targetYaw = e.y;
                float targetPitch = Mathf.DeltaAngle(0f, e.x);

                float t = 1f - Mathf.Exp(-lockCameraSmooth * Time.deltaTime);
                yaw = Mathf.LerpAngle(yaw, targetYaw, t);
                pitch = Mathf.Lerp(pitch, targetPitch, t);
            }
        }
        else
        {
            // sin objetivo = camara libre
            Vector2 look = _inputs.look;
            if (look.sqrMagnitude > 0.0001f)
            {
                float sens = _inputs.LookIsGamepad
                    ? gamepadSensitivity * Time.deltaTime
                    : mouseSensitivity;

                yaw += look.x * sens;
                pitch -= look.y * sens;
            }
        }

        pitch = Mathf.Clamp(pitch, bottomClamp, topClamp);
        cinemachineCameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleCursorLock()
    {
        // al hacer clic, bloquea el cursor de nuevo
        if (Time.timeScale > 0f &&
            Cursor.lockState != CursorLockMode.Locked &&
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // gira al jugador hacia el objetivo fijado (solo horizontal)
    private void FaceTarget()
    {
        Vector3 toTarget = targetLock.AimPoint - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.01f)
        {
            float faceAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, faceAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    // inicia el esquive calculando la direccion y avisando a las animaciones
    private void TryStartDodge(Vector2 move)
    {
        if (dodgeCooldownTimer > 0f) return;

        if (move.sqrMagnitude > 0.15f * 0.15f)
        {
            // hacia donde apunta el stick, relativo a la camara
            Vector3 input = new Vector3(move.x, 0f, move.y).normalized;
            dodgeDir = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * input;
        }
        else
        {
            // sin stick: paso atras, alejandose del objetivo
            Vector3 away = transform.position - targetLock.AimPoint;
            away.y = 0f;
            dodgeDir = away.sqrMagnitude > 0.01f ? away.normalized : -transform.forward;
        }

        isDodging = true;
        dodgeTimer = 0f;

        Vector3 local = transform.InverseTransformDirection(dodgeDir);
        Vector2 localDir = new Vector2(local.x, local.z);

        Debug.Log($"Esquive: direccion local {localDir}"); // quitar cuando este la animacion
        DodgeStarted?.Invoke(localDir); // avisamos a la animacion
    }

    // avanza el esquive un frame segun la curva y devuelve cuanto desplazarse :p
    private Vector3 StepDodge()
    {
        float previous = dodgeCurve.Evaluate(dodgeTimer / dodgeDuration);

        dodgeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(dodgeTimer / dodgeDuration);
        float current = dodgeCurve.Evaluate(t);

        // al terminar, salimos del estado y aplicamos cooldown
        if (t >= 1f)
        {
            isDodging = false;
            dodgeCooldownTimer = dodgeCooldown;
        }

        return dodgeDir * ((current - previous) * dodgeDistance);
    }

    //funcion que devuelve si el jugador es invulnerable o no, se usa para el esquive 
    // en un futuro cuando se tenga el sistema de vida del jugador recordar usar esta funcion para que el jugador no reciba daño mientras esquiva
    public bool IsInvulnerable()
    {
        return isDodging && dodgeTimer < dodgeDuration * dodgeInvulnerableFraction;
    }


}