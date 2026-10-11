using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))] // Asegura que el GameObject tenga un CharacterController

//este script se encarga de mover al jugador y controlar la camara, utilizando los inputs recibidos del script PlayerInputHandler (aunque deberia hacerlo mas escalable a futuro para mas botones)
public class PlayerMovement : MonoBehaviour
{
    private CharacterController _char; // referencia al CharacterController del jugador
    private PlayerInputHandler _inputs; // referencia al script PlayerInputHandler que recibe los inputs del jugador
    private Transform cameraTransform; // referencia a la camara principal del juego, para poder mover al jugador en la direccion de la camara


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

    private float verticalVelocity; // velocidad vertical del jugador, utilizada para aplicar la gravedad y el salto
    private float turnSmoothVelocity; // velocidad de suavizado para la rotación del jugador
    private float yaw, pitch; // variables para almacenar la rotación de la cámara en los ejes Y (yaw) y X (pitch)


    // Funciones de Unity
    private void Awake()
    {
        _char = GetComponent<CharacterController>();
        _inputs = GetComponent<PlayerInputHandler>();


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

    private void Update()
    {
        HandleCursorLock(); //bloqueamos el cursor al iniciar el movimiento


        Vector3 horizontal = Vector3.zero;
        Vector2 move = _inputs.move;

        //si el input de movimiento es mayor a un umbral, se calcula la dirección del movimiento y se aplica la rotación del jugador hacia esa dirección
        if (move.sqrMagnitude > 0.15f * 0.15f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            horizontal = dir * (playerSpeed * Mathf.Clamp01(move.magnitude));
        }

        // Gravedad y salto
        if (_char.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (_inputs.jump && _char.isGrounded)
            verticalVelocity = Mathf.Sqrt(playerJump * -2f * playerGravity);

        _inputs.jump = false;
        verticalVelocity += playerGravity * Time.deltaTime;

        // Un solo Move por frame
        _char.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    private void LateUpdate()
    {
        //si no hay un objetivo de cámara de Cinemachine asignado, no se hace nada
        if (cinemachineCameraTarget == null) return;

        Vector2 look = _inputs.look;

        // si el input de la cámara es mayor a un umbral, se calcula la rotación de la cámara y se aplica al objetivo de cámara de Cinemachine
        if (look.sqrMagnitude > 0.0001f)
        {
            // El ratón ya es un delta por frame; el stick necesita deltaTime
            float sens = _inputs.LookIsGamepad
                ? gamepadSensitivity * Time.deltaTime
                : mouseSensitivity;

            yaw += look.x * sens;
            pitch -= look.y * sens; // ratón arriba = mirar arriba
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