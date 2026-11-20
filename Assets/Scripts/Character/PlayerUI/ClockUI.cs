using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour
{
    [Header("Referencias Visuales")]
    [SerializeField] private Image circleFill; // El círculo que se vacía
    [SerializeField] private Image iconImage;  // El sol o la luna

    [Header("Sprites del Icono")]
    [SerializeField] private Sprite dayIcon;   // Sprite del sol
    [SerializeField] private Sprite nightIcon; // Sprite de la luna

    [Header("Colores del Círculo")]
    [SerializeField] private Color dayColor = Color.yellow;
    [SerializeField] private Color nightColor = new Color(0.2f, 0.2f, 0.5f); // Azul oscuro

    [Header("Visibilidad por Escena")]
    [SerializeField] private string[] hiddenInScenes; // Nombres exactos de escenas sin ícono

    private bool iconHiddenInCurrentScene; // indica si el icono esta oculto en la escena actual 

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EvaluateIconVisibility(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateIconVisibility(scene.name);
    }
    //metodo para actualizar el reloj segun la esceana filtrada para no mostrar el icono
    private void EvaluateIconVisibility(string sceneName)
    {
        iconHiddenInCurrentScene = false;

        if (hiddenInScenes != null)
        {
            for (int i = 0; i < hiddenInScenes.Length; i++)
            {
                if (hiddenInScenes[i] == sceneName)
                {
                    iconHiddenInCurrentScene = true;
                    break;
                }
            }
        }

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(!iconHiddenInCurrentScene);
        }
    }

    private void Update()
    {
        if (DayNightController.Instance == null) return;

        DayNightController controller = DayNightController.Instance;

        if (circleFill != null)
        {
            circleFill.fillAmount = 1f - controller.TimeProgress;
        }

        if (iconImage != null && !iconHiddenInCurrentScene)
        {
            if (!iconImage.gameObject.activeSelf)
            {
                iconImage.gameObject.SetActive(true);
            }

            iconImage.sprite = controller.IsDayTime ? dayIcon : nightIcon;
        }

        if (circleFill != null)
        {
            circleFill.color = controller.IsDayTime ? dayColor : nightColor;
        }
    }
}