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




    // Variables de configuración del jugador y la cámara
    [Header("Player")]
    [SerializeField] private float playerSpeed = 5f; // velocidad del jugador
    [SerializeField] private float playerJump = 1.2f;       // altura del salto en metros
    [SerializeField] private float turnSmoothTime = 0.1f; // tiempo de suavizado para la rotación del jugador
    [SerializeField] private float playerGravity = -20f; // gravedad aplicada al jugador

    // Variables de configuración de la cámara

    [Header("Cámara")]
    [SerializeField] private Transform cinemachineCameraTarget; // referencia al gameObject de cameraRoot que es hijo del PJ y que se encarga de mover la cámara con Cinemachine
    [SerializeField] private float mouseSensitivity = 0.1f; // sensibilidad del ratón para mover la cámara
    [SerializeField] private float gamepadSensitivity = 150f; // sensibilidad del gamepad para mover la cámara
    [SerializeField] private float bottomClamp = -30f;      // es lo maximo que se puede mirar hacia arriba
    [SerializeField] private float topClamp = 60f;          // es lo maximo que se puede mirar hacia abajo
    [SerializeField] private float lockCameraSmooth = 8f; // suavizado de la cámara al fijar un objetivo, para que no se mueva bruscamente

    private float verticalVelocity; // velocidad vertical del jugador, utilizada para aplicar la gravedad y el salto
    private float turnSmoothVelocity; // velocidad de suavizado para la rotación del jugador
    private float yaw, pitch; // variables para almacenar la rotación de la cámara en los ejes Y (yaw) y X (pitch)


    // Funciones de Unity
    private void Awake()
    {
        _char = GetComponent<CharacterController>();
        _inputs = GetComponent<PlayerInputHandler>();
        targetLock = GetComponent<TargetLock>();


        // si no hay ninguna cámara con el tag MainCamera, se desactiva este script y se muestra un error en la consola
        if (Camera.main == null)
        {
            Debug.LogError("No hay ninguna cámara con el tag MainCamera.", this);
            enabled = false;
            return;
        }
        cameraTransform = Camera.main.transform;
    }


    private void Start()
    {
        // Inicializa la rotación de la cámara si hay un objetivo de cámara de Cinemachine asignado
        if (cinemachineCameraTarget != null)
            yaw = cinemachineCameraTarget.eulerAngles.y;

        //desactivamos el cursor al iniciar el juego para que no se vea y se pueda mover la cámara con el ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // el update se encarga de mover al jugador y aplicar la gravedad y el salto, ademas de fijar la rotación del jugador hacia el enemigo si hay un objetivo fijado
    private void Update()
    {
        HandleCursorLock(); // Maneja el bloqueo del cursor al hacer clic en la pantalla

        Vector2 move = _inputs.move; //
        Vector3 horizontal = Vector3.zero;
        bool locked = targetLock != null && targetLock.IsLocked;


        //si el objetivo esta fijado, el jugador se mueve en strafe y mira al enemigo, si no esta fijado, el jugador se mueve libremente y mira hacia donde se mueve
        if (locked)
        {
            // Con objetivo: el personaje mira al enemigo (solo horizontal :p )
            Vector3 toTarget = targetLock.AimPoint - transform.position;
            toTarget.y = 0f;

            //si la distancia al objetivo es mayor a 0.1, se rota el jugador hacia el objetivo suavemente
            if (toTarget.sqrMagnitude > 0.01f)
            {
                float faceAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y, faceAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // si el jugador se mueve, se aplica la velocidad en la dirección de la cámara, para que se mueva en strafe y no hacia el enemigo
            if (move.sqrMagnitude > 0.15f * 0.15f)
            {
                Vector3 input = Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);
                horizontal = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * input * playerSpeed;
            }
        }

        //si no hay objetivo fijado, el jugador se mueve libremente y mira hacia donde se mueve
        else if (move.sqrMagnitude > 0.15f * 0.15f)
        {
            // Sin objetivo el jugador tiene movimiento libre (igual que antes xd)
            float targetAngle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            horizontal = dir * (playerSpeed * Mathf.Clamp01(move.magnitude));
        }

        // manejamos la gravedad y salto del jugador si esta en el suelo
        if (_char.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (_inputs.jump && _char.isGrounded)
            verticalVelocity = Mathf.Sqrt(playerJump * -2f * playerGravity);

        _inputs.jump = false;
        verticalVelocity += playerGravity * Time.deltaTime;

        _char.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    // esta funcion se encarga de mover la cámara con el ratón o el gamepad, y de fijar la cámara sobre el enemigo si hay un objetivo fijado
    private void LateUpdate()
    {
        // si no hay un objetivo de cámara de Cinemachine asignado, no se hace nada
        if (cinemachineCameraTarget == null) return;

        if (targetLock != null && targetLock.IsLocked)
        {
            // Con objetivo: la cámara se centra sola en el enemigo
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
            // Sin objetivo = cámara libre
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


    //funcion para bloquear el cursor al iniciar el juego y al hacer clic en la pantalla, para que no se vea y se pueda mover la cámara con el ratón
    private void HandleCursorLock()
    {
        // Al hacer clic en el juego, vuelve a bloquear el cursor
        if (Cursor.lockState != CursorLockMode.Locked &&
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}