using UnityEngine;

public class DayNightController : MonoBehaviour
{
    public static DayNightController Instance { get; private set; }

    [Header("Configuración de Tiempos")]
    [Tooltip("Duración en minutos de la fase diurna")]
    [SerializeField] private float dayDurationMinutes = 10f;
    [Tooltip("Duración en minutos de la fase nocturna")]
    [SerializeField] private float nightDurationMinutes = 10f;

    [Header("Referencias Visuales")]
    [SerializeField] private Light directionalLight;

    [Header("Configuración de Rotación")]
    [Tooltip("Si está activo, toma exactamente la rotación (X, Y, Z) que tenga la luz en el Inspector al iniciar como su punto de partida.")]
    [SerializeField] private bool usarRotacionInicialDelEditor = true;
    [Tooltip("Si desactivas la opción de arriba, puedes escribir los ángulos base aquí manualmente.")]
    [SerializeField] private Vector3 rotacionPersonalizadaBase;

    // Estados internos
    private float currentTime;
    private float currentPhaseDuration;
    private bool isDayTime = true;
    private Vector3 baseRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        currentPhaseDuration = dayDurationMinutes * 60f;
        currentTime = 0f;

        if (directionalLight != null)
        {
            // Guardamos la rotación con la que empieza el objeto en la escena (X, Y y Z)
            if (usarRotacionInicialDelEditor)
            {
                baseRotation = directionalLight.transform.eulerAngles;
            }
            else
            {
                baseRotation = rotacionPersonalizadaBase;
            }
        }
    }

    private void Update()
    {
        ManejarTiempo();
    }

    private void ManejarTiempo()
    {
        currentTime += Time.deltaTime;
        float progress = currentTime / currentPhaseDuration;

        ActualizarIluminacion(progress);

        if (currentTime >= currentPhaseDuration)
        {
            CambiarFase();
        }
    }

    private void CambiarFase()
    {
        isDayTime = !isDayTime;
        currentTime = 0f;

        if (isDayTime)
        {
            currentPhaseDuration = dayDurationMinutes * 60f;
            GameManager.Instance.currentTimeOfDay = TimeofDay.Day;
            Debug.Log("Ha comenzado el DÍA");
        }
        else
        {
            currentPhaseDuration = nightDurationMinutes * 60f;
            GameManager.Instance.currentTimeOfDay = TimeofDay.Night;
            Debug.Log("Ha comenzado la NOCHE");
        }
    }

    private void ActualizarIluminacion(float progress)
    {
        if (directionalLight == null) return;

        // Calculamos cuánto debe avanzar el eje X según el progreso (0 a 180 para el día, 180 a 360 para la noche)
        float targetAngleX = isDayTime ? Mathf.Lerp(0f, 180f, progress) : Mathf.Lerp(180f, 360f, progress);

        // Sumamos el desplazamiento del tiempo a la rotación base en X, respetando los ejes Y y Z originales
        float finalX = baseRotation.x + targetAngleX;

        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(finalX, baseRotation.y, baseRotation.z));
    }
}