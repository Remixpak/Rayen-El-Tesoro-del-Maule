using UnityEngine;

// este script muestra el set de botones de teclado o el de mando segun el ultimo dispositivo usado
// asi el jugador ve siempre los prompts correctos en pantalla
public class InputPromptSet : MonoBehaviour
{
    [SerializeField] private GameObject keyboardSet; // objeto con los prompts de teclado (teclas)
    [SerializeField] private GameObject gamepadSet; // objeto con los prompts de mando (botones)

    private void OnEnable()
    {
        // nos suscribimos al evento del tracker para enterarnos cuando cambie el dispositivo
        InputDeviceTracker.Changed += Refresh;
        Refresh(); // al abrir la seccion, se ajusta al dispositivo actual
    }

    private void OnDisable()
    {
        // nos desuscribimos para no dejar referencias colgando
        InputDeviceTracker.Changed -= Refresh;
    }

    // se llama cuando cambia el dispositivo o al activarse el objeto
    private void Refresh()
    {
        bool gamepad = InputDeviceTracker.UsingGamepad; // miramos si el ultimo dispositivo usado es mando

        // mostramos solo el set que toca (y ocultamos el otro)
        if (keyboardSet != null) keyboardSet.SetActive(!gamepad);
        if (gamepadSet != null) gamepadSet.SetActive(gamepad);
    }
}