using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// este script es el gestor central de menus: abre y cierra pantallas (pausa, diario, etc.),
// las apila para poder volver hacia atras, pausa el juego y bloquea/libera el cursor segun haga falta
public class MenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInputHandler inputs; // input del jugador
    [SerializeField] private GameObject backdrop; // fondo oscuro que se pone detras del menu (opcional)

    [Header("Pantallas de juego")]
    [SerializeField] private MenuScreen pauseScreen; // pantalla de pausa (P / Start)
    [SerializeField] private MenuScreen bookScreen; // pantalla del libro/diario (Tab / Select)

    [Header("Menú principal (opcional)")]
    private MenuScreen rootScreen; // si se asigna, siempre esta abierta y no se cierra (basicamente abre siempre un menu al iniciar, podria servir con el libro para tutorial o no se)

    private readonly List<MenuScreen> stack = new List<MenuScreen>(); // pila de pantallas abiertas (la de arriba es la que se ve)
    private bool inMenuMode; // para saber si ya estamos en modo menu (y no repetir la llamada a SetMenuMode)

    // devuelve true si hay alguna pantalla abierta
    public bool IsOpen
    {
        get { return stack.Count > 0; }
    }

    // devuelve la pantalla que esta arriba del todo (la visible)
    private MenuScreen Current
    {
        get { return stack.Count > 0 ? stack[stack.Count - 1] : null; }
    }

    private void Start()
    {
        // si no hay input asignado, avisamos por consola y desactivamos el script
        if (inputs == null)
        {
            Debug.LogError("MenuManager: falta asignar el PlayerInputHandler.", this);
            enabled = false;
            return;
        }

        // si hay una pantalla raiz, la abrimos al empezar
        if (rootScreen != null)
        {
            Open(rootScreen);
        }
    }

    private void Update()
    {
        // si el jugador pulsa pausa, abrimos o cerramos el menu de pausa
        if (inputs.pause)
        {
            inputs.pause = false;
            OnPausePressed();
        }

        // si el jugador pulsa el boton del diario, abrimos o cerramos el libro
        if (inputs.openJournal)
        {
            inputs.openJournal = false;
            OnJournalPressed();
        }

        // si no hay menu abierto, limpiamos los inputs de navegacion y no hacemos nada mas
        if (!IsOpen)
        {
            inputs.tabPrev = false;
            inputs.tabNext = false;
            inputs.cancel = false;
            return;
        }

        // si el jugador pulsa la pestaña anterior, avisamos a la pantalla actual
        if (inputs.tabPrev)
        {
            inputs.tabPrev = false;
            Current.OnTabPrev();
        }

        // si el jugador pulsa la pestaña siguiente, avisamos a la pantalla actual
        if (inputs.tabNext)
        {
            inputs.tabNext = false;
            Current.OnTabNext();
        }

        // si el jugador pulsa cancelar/volver
        if (inputs.cancel)
        {
            inputs.cancel = false;

            // si la pantalla no gestiona "volver", retrocedemos en la pila
            if (!Current.OnCancel())
            {
                Back();
            }
        }
    }

    // se llama al pulsar pausa (P / Start)
    private void OnPausePressed()
    {
        // si hay pantalla raiz, Start no hace nada
        if (rootScreen != null)
        {
            return;
        }

        // si hay algo abierto, cerramos todo; si no, abrimos la pantalla de pausa
        if (IsOpen)
        {
            CloseAll();
        }
        else
        {
            Open(pauseScreen);
        }
    }

    // se llama al pulsar el boton del diario (Tab / Select)
    private void OnJournalPressed()
    {
        // si hay pantalla raiz, no hacemos nada
        if (rootScreen != null)
        {
            return;
        }

        // si no hay nada abierto, abrimos el libro; si ya esta el libro abierto, cerramos todo
        if (!IsOpen)
        {
            Open(bookScreen);
        }
        else if (Current == bookScreen)
        {
            CloseAll(); // con la pausa abierta, Tab no hace nada
        }
    }

    // ---- API publica (la usan los botones de las pantallas) ----

    // abre una pantalla nueva y la pone arriba de la pila
    public void Open(MenuScreen screen)
    {
        // si la pantalla es null, avisamos y no hacemos nada
        if (screen == null)
        {
            Debug.LogError("MenuManager: se intentó abrir una pantalla null.", this);
            return;
        }

        // si ya es la pantalla actual, no hacemos nada
        if (Current == screen)
        {
            return;
        }

        // si habia otra pantalla abierta, la ocultamos
        if (Current != null)
        {
            Hide(Current);
        }

        stack.Add(screen); // metemos la nueva en la pila
        Show(screen); // la mostramos
        ApplyState(); // aplicamos pausa/cursor
    }

    // vuelve a la pantalla anterior (o cierra el menu si no hay ninguna)
    public void Back()
    {
        // si no hay nada abierto o estamos en la raiz, no hacemos nada
        if (!IsOpen || Current == rootScreen)
        {
            return;
        }

        Hide(Current); // ocultamos la de arriba
        stack.RemoveAt(stack.Count - 1); // la quitamos de la pila

        // si queda alguna, mostramos la de abajo
        if (IsOpen)
        {
            Show(Current);
        }

        ApplyState();
    }

    // cierra todo y vuelve al juego (si hay pantalla raiz, vuelve a ella)
    public void CloseAll()
    {
        // si no hay nada abierto o estamos en la raiz, no hacemos nada
        if (!IsOpen || Current == rootScreen)
        {
            return;
        }

        Hide(Current); // solo la de arriba estaba visible

        // vamos quitando pantallas de la pila hasta llegar a la raiz (o vaciarla)
        while (IsOpen && Current != rootScreen)
        {
            stack.RemoveAt(stack.Count - 1);
        }

        // si queda alguna (la raiz), la mostramos
        if (IsOpen)
        {
            Show(Current);
        }

        ApplyState();
    }

    // ---- Internos ----

    // muestra una pantalla y selecciona su primer elemento
    private void Show(MenuScreen screen)
    {
        screen.gameObject.SetActive(true);
        screen.OnShown();
        SelectFirst(screen);
    }

    // oculta una pantalla
    private void Hide(MenuScreen screen)
    {
        screen.OnHidden();
        screen.gameObject.SetActive(false);
    }

    // selecciona el primer elemento de una pantalla para poder navegar con mando
    private static void SelectFirst(MenuScreen screen)
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(screen.GetFirstSelected());
    }

    // aplica el estado del menu: pausa, audio, fondo, cursor y modo menu
    private void ApplyState()
    {
        bool open = IsOpen;
        bool pauses = false;

        // si alguna pantalla de la pila pausa el juego, pausamos
        foreach (MenuScreen screen in stack)
        {
            if (screen.PausesGame)
            {
                pauses = true;
            }
        }

        Time.timeScale = pauses ? 0f : 1f; // pausamos o no el juego
        AudioListener.pause = pauses; // pausamos o no el audio (los sonidos de UI deben marcar "Ignore Listener Pause" en su AudioSource)

        if (backdrop != null)
        {
            backdrop.SetActive(open); // mostramos u ocultamos el fondo oscuro
        }

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked; // liberamos o bloqueamos el cursor
        Cursor.visible = open;

        // avisamos al input de que estamos en modo menu o no (solo cuando cambia)
        if (open != inMenuMode)
        {
            inMenuMode = open;
            inputs.SetMenuMode(open);
        }
    }

    // por seguridad: si se sale de la escena con un menu abierto, no dejamos el juego congelado
    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}