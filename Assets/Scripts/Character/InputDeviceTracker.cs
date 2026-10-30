using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// este script recuerda si el jugador uso por ultima vez teclado/raton o mando
// y avisa a quien escuche cuando cambia de dispositivo (para cambiar los prompts en pantalla)
public static class InputDeviceTracker
{
    public static bool UsingGamepad { get; private set; } // true si el ultimo dispositivo usado es mando
    public static event Action Changed; // evento que se dispara cuando cambia el dispositivo

    private static IDisposable listener; // referencia al listener del Input System (para poder liberarlo)

    // se llama al cargar el subsistema: resetea las variables estaticas (importante al recargar el dominio en el editor)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        listener?.Dispose(); // liberamos el listener anterior si lo habia
        listener = null;
        UsingGamepad = false;
        Changed = null;
    }

    // se llama antes de cargar la primera escena: empieza a escuchar cualquier boton
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // nos suscribimos a "cualquier boton pulsado" para detectar el dispositivo
        listener = InputSystem.onAnyButtonPress.Call(OnAnyButton);
    }

    // se llama cada vez que se pulsa cualquier boton
    private static void OnAnyButton(InputControl control)
    {
        InputDevice device = control.device; // dispositivo del boton pulsado

        bool isGamepad = device is Gamepad; // ¿es un mando?
        bool isKeyboardMouse = device is Keyboard || device is Mouse; // ¿es teclado o raton?

        // si el dispositivo no nos interesa, lo ignoramos
        if (!isGamepad && !isKeyboardMouse)
        {
            return;
        }

        // si el tipo no ha cambiado, no hacemos nada
        if (isGamepad == UsingGamepad)
        {
            return;
        }

        // actualizamos el estado y avisamos a quien escuche
        UsingGamepad = isGamepad;
        Changed?.Invoke();
    }
}