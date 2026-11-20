using UnityEngine;

public class DayNightController : MonoBehaviour
{
    public static DayNightController Instance { get; private set; }

    [Header("Configuraci�n de Tiempos")]
    [Tooltip("Duraci�n en minutos de la fase diurna")]
    [SerializeField] private float dayDurationMinutes = 10f;
    [Tooltip("Duraci�n en minutos de la fase nocturna")]
    [SerializeField] private float nightDurationMinutes = 10f;

    [Header("Referencias Visuales")]
    [SerializeField] private Light directionalLight;

    [Header("Configuraci�n de Rotaci�n")]
    [Tooltip("Si est� activo, toma exactamente la rotaci�n (X, Y, Z) que tenga la luz en el Inspector al iniciar como su punto de partida.")]
    [SerializeField] private bool usarRotacionInicialDelEditor = true;
    [Tooltip("Si desactivas la opci�n de arriba, puedes escribir los �ngulos base aqu� manualmente.")]
    [SerializeField] private Vector3 rotacionPersonalizadaBase;

    // Estados internos
    private float currentTime;
    private float currentPhaseDuration;
    private bool isDayTime = true;
    private Vector3 baseRotation;

    //cositas que agregue para hacer la conexion con el CLOCKUI 
    public bool IsDayTime => isDayTime; //propiedad para que otros scripts puedan consultar si es dia o noceh
    public float TimeProgress => currentPhaseDuration > 0 ? (currentTime / currentPhaseDuration) : 0f;
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
            // Guardamos la rotaci�n con la que empieza el objeto en la escena (X, Y y Z)
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
            Debug.Log("Ha comenzado el D�A");
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

        // Calculamos cu�nto debe avanzar el eje X seg�n el progreso (0 a 180 para el d�a, 180 a 360 para la noche)
        float targetAngleX = isDayTime ? Mathf.Lerp(0f, 180f, progress) : Mathf.Lerp(180f, 360f, progress);

        // Sumamos el desplazamiento del tiempo a la rotaci�n base en X, respetando los ejes Y y Z originales
        float finalX = baseRotation.x + targetAngleX;

        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(finalX, baseRotation.y, baseRotation.z));
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}