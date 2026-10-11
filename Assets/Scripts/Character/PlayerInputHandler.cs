using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    //este script se encarga de recibir los inputs del jugador y almacenarlos en variables para que otros scripts puedan acceder a ellos

    [Header("Input Values")]
    public Vector2 move; //almacena el input de movimiento del jugador
    public Vector2 look; //almacena el input de la camara del jugador
    public bool jump; //almacena el input de salto del jugador

    // actualiza el valor de move cada vez que se recibe un input de movimiento
    public void OnMove(InputValue value) => move = value.Get<Vector2>();

    // actualiza el valor de look cada vez que se recibe un input de la camara
    public void OnLook(InputValue value) => look = value.Get<Vector2>();

    //funcion que se llama cuando se recibe un input de salto, si el input es presionado, se actualiza el valor de jump a true
    public void OnJump(InputValue value)
    {
        if (value.isPressed) jump = true; //si el input es presionado, se actualiza el valor de jump a true
    }

    //funcion que se llama cuando se recibe un input de salto, si el input es presionado, se actualiza el valor de jump a false
    public bool LookIsGamepad =>
        Gamepad.current != null &&
        Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.0001f;
}