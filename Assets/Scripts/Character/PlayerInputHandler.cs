using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    //este script se encarga de recibir los inputs del jugador y almacenarlos en variables para que otros scripts puedan acceder a ellos

    [Header("Juego")]
    public Vector2 move; //almacena el input de movimiento del jugador
    public Vector2 look; //almacena el input de la camara del jugador
    public bool jump; //almacena el input de salto del jugador
    public bool LockOn; //almacena el input de lock on del jugador
    public bool attack; //almacena el input de ataque del jugador
    public bool swtichWeapon; // almacena el input del cambio de arma del jugador
    public bool interact;     // E / B o círculo

    [Header("Menus")]
    public bool pause;//almacena el input para pausar el juego(P/start)
    public bool openJournal; // Tab/select
    public bool tabPrev;//almacena el input para cambiar el canvas UI del menu ( Q/LB)
    public bool tabNext;// E/RB
    public bool cancel; //esc/click derecho/ circulo o B

    private PlayerInput playerInput;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

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


    //funcion para recbiri el input de fijar enemigo
    public void OnLockOn(InputValue value)
    {
        // basicamente si el input es presionado el lock on es verdadero y se ejecuta
        if (value.isPressed)
        {
            LockOn = true;
        }
    }

    //funcion para recibir el input de ataque, si el input es presionado, se actualiza el valor de attack a true
    public void OnAttack(InputValue value)
    {
        // basicamente si el input es presionado el ataque es verdaedero y se ejecuta
        if (value.isPressed)
        {
            attack = true;
        }

    }

    //funcion par recibir el input del cambio de arma
    public void OnSwitchWeapon(InputValue value)
    {
        //basicamente si el input es presionado se cambia el arma   
        if (value.isPressed)
        {
            swtichWeapon = true;
        }
    }

    //funcion parar recibir el input del pausa
    public void OnPause(InputValue value)
    {
        //si se presiona el boton de pausa el juego se pausa
        if (value.isPressed)
        {
            pause = true;
        }
    }

    //funcion para pasar pasar al siguiente canvas UI  estando en pausa 
    public void OnTabPrev(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("TabPrev recibido"); if (value.isPressed) tabPrev = true;
            tabPrev = true; 
        }
    }

    //funcion para pasar pasar al siguiente canvas UI  estando en pausa 
    public void OnTabNext(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("TabNext recibido"); if (value.isPressed) tabNext = true;
            tabNext = true;
        }
    }

    //funcion para cancelar acciones dentro del canvas
    public void OnCancel(InputValue value)
    {
        if (value.isPressed)
        {
            cancel = true;
        }
    }

    //intercambiar entre modos del menu
    public void SetMenuMode(bool menuOpen)
    {
        ResetInputs();
        playerInput.SwitchCurrentActionMap(menuOpen ? "UI" : "Player");
    }

    public void OnOpenJournal(InputValue value)
    {
        if (value.isPressed)
        {
            openJournal = true;
        }
    }


    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            interact = true;
        }
    }



    //resteamos las acciones del jugador al cerrar el menu
    private void ResetInputs()
    {
        move = Vector2.zero;
        look = Vector2.zero;
        jump = false;
        attack = false;
        LockOn = false;
        swtichWeapon = false;
        pause = false;
        tabPrev = false;
        tabNext = false;
        interact = false;
    }

}