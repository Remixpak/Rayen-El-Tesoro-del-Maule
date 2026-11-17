using UnityEngine;
using UnityEngine.Events; // Necesario para usar UnityEvent en el Inspector

public class Switch : MonoBehaviour
{
    [Header("Configuración de Material")]
    [Tooltip("El componente Renderer del objeto cuyo material cambiará.")]
    [SerializeField] private Renderer targetRenderer;
    [Tooltip("El nuevo material que se aplicará al presionar el switch.")]
    [SerializeField] private Material pressedMaterial;

    [Header("Eventos (Estilo Botón de UI)")]
    [Tooltip("Métodos que se ejecutarán al activar el switch por primera vez.")]
    [SerializeField] private UnityEvent onSwitchActivated;

    [Header("Estado")]
    [SerializeField] private bool isPressed = false; // Controla si ya fue presionado

    [Header("Detección")]
    [SerializeField] private string playerTag = "Player"; // Tag para identificar al jugador

    private void Start()
    {
        // Si no asignaste el renderer en el inspector, lo busca en el mismo objeto
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }

    // Usamos OnTriggerEnter. Recuerda marcar el Collider del switch como "Is Trigger".
    private void OnTriggerEnter(Collider other)
    {
        // Si ya fue presionado, ignoramos cualquier colisión posterior
        if (isPressed) return;

        // Comprobamos si el objeto que colisiona es el jugador
        if (other.CompareTag(playerTag))
        {
            PressSwitch();
        }
    }

    private void PressSwitch()
    {
        isPressed = true; // Marcamos que ya fue presionado para que no se repita

        // 1. Cambiar el material
        if (targetRenderer != null && pressedMaterial != null)
        {
            targetRenderer.material = pressedMaterial;
        }

        // 2. Ejecutar los métodos asignados desde el Inspector (similar al onClick)
        onSwitchActivated?.Invoke();

        Debug.Log("¡Switch presionado y activado!");
    }
}