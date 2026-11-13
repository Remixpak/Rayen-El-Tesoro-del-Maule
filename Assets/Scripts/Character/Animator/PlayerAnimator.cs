using UnityEngine;

// alimenta el Animator del modelo con lo que hace el jugador (movimiento)
// los nombres de los parametros deben coincidir exactamente con los del Animator Controller


//recordar que por el momento el animator funciona con velocidad del movimiento en el locomation por lo que si se altera la velocidad del player se debe cambiar tambien en el locomotion del animation controller :p
[RequireComponent(typeof(CharacterController))]
public class PlayerAnimator : MonoBehaviour
{
    private enum SpeedMode
    {
        Normalized,      // 0 = quieto, 1 = velocidad maxima
        MetersPerSecond  // velocidad real en metros por segundo
    }

    [Header("Referencias")]
    [SerializeField] private Animator animator; // el Animator del modelo (hijo del jugador). Si esta vacio, lo busca en los hijos

    [Header("Parámetros del Animator")]
    [SerializeField] private string movingParameter = "IsMoving"; // Bool: true si se mueve (vacio si no existe)
    [SerializeField] private string speedParameter = "";          // Float opcional para un blend tree (vacio si no existe)

    [Header("Velocidad")]
    [SerializeField] private SpeedMode speedMode = SpeedMode.Normalized;
    [SerializeField] private float maxSpeed = 5f;          // debe coincidir con "Player Speed" de PlayerMovement
    [SerializeField] private float dampTime = 0.1f;        // suavizado del parametro Float
    [SerializeField] private float movingThreshold = 0.1f; // m/s a partir de los cuales cuenta como "en movimiento"

    private CharacterController controller;
    private int movingHash;
    private int speedHash;
    private bool hasMoving;
    private bool hasSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("PlayerAnimator: no encuentra el Animator. El modelo debe ser hijo del jugador.", this);
            enabled = false;
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("PlayerAnimator: el Animator del modelo no tiene un Animator Controller asignado.", this);
            enabled = false;
            return;
        }

        animator.applyRootMotion = false; // el movimiento lo hace el CharacterController

        hasMoving = HasParameter(movingParameter, AnimatorControllerParameterType.Bool);
        hasSpeed = HasParameter(speedParameter, AnimatorControllerParameterType.Float);

        movingHash = Animator.StringToHash(movingParameter);
        speedHash = Animator.StringToHash(speedParameter);
    }

    private void Update()
    {
        // con el juego congelado (pausa o dialogo) no actualizamos nada
        if (Time.deltaTime <= 0f)
        {
            return;
        }

        Vector3 velocity = controller.velocity;
        velocity.y = 0f; // solo cuenta la velocidad horizontal
        float speed = velocity.magnitude;

        if (hasMoving)
        {
            animator.SetBool(movingHash, speed > movingThreshold);
        }

        if (hasSpeed)
        {
            float value = speed;

            if (speedMode == SpeedMode.Normalized)
            {
                value = Mathf.Clamp01(speed / Mathf.Max(0.01f, maxSpeed));
            }

            animator.SetFloat(speedHash, value, dampTime, Time.deltaTime);
        }
    }

    // comprueba que el parametro existe y avisa si no (los nombres deben ser exactos)
    private bool HasParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
            {
                return true;
            }
        }

        Debug.LogWarning("PlayerAnimator: el Animator no tiene un parámetro " + type + " llamado '" + parameterName + "'. Revisa la pestaña Parameters del Animator Controller.", this);
        return false;
    }
}