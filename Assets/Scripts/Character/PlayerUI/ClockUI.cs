using UnityEngine;
using UnityEngine.UI;
using TMPro; 


//basicamente este scritp se encarga de hacer la conexion visual con el script de daynigthcyckle para que podamos ver en el hud el ciclo de dia y noche 
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

    private void Update()
    {
        // Si no hay instancia del controlador, no hacemos nada
        if (DayNightController.Instance == null) return;

        DayNightController controller = DayNightController.Instance;

        // Actualizamos el círculo con el fill de 1 a 0 segun el progreso del tiempo 
        if (circleFill != null)
        {
            circleFill.fillAmount = 1f - controller.TimeProgress;
        }

        // Cambiamos el color y el icono según si es de día o de noche
        if (controller.IsDayTime)
        {
            if (circleFill != null) circleFill.color = dayColor;
            if (iconImage != null) iconImage.sprite = dayIcon;
        }
        else
        {
            if (circleFill != null) circleFill.color = nightColor;
            if (iconImage != null) iconImage.sprite = nightIcon;
        }
    }
}