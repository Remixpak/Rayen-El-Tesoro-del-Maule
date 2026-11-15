using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInputHandler inputs; // input del jugador
    [SerializeField] private GameObject backdrop; // fondo oscuro que se pone detras del menu (opcional)

    [Header("Pantallas de juego")]
    [SerializeField] private MenuScreen pauseScreen; // pantalla de pausa (P / Start)
    [SerializeField] private MenuScreen bookScreen; // pantalla del libro/diario (Tab / Select)

    [Header("Menú principal (opcional)")]
    [SerializeField] private MenuScreen rootScreen; // si se asigna, siempre esta abierta y no se cierra

    private readonly List<MenuScreen> stack = new List<MenuScreen>(); // pila de pantallas abiertas
    private bool inMenuMode;
    private bool isInitialized;

    public bool IsOpen => stack.Count > 0;
    private MenuScreen Current => stack.Count > 0 ? stack[stack.Count - 1] : null;

    private void Start()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (isInitialized) return;

        if (inputs == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            inputs = GameManager.Instance.player.GetComponent<PlayerInputHandler>();
        }

        if (inputs == null)
        {
            return;
        }

        if (rootScreen != null)
        {
            Open(rootScreen);
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            TryInitialize();
            if (!isInitialized) return;
        }

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
            inputs.advance = false;
            inputs.cancel = false;
            return;
        }

        if (inputs.tabPrev)
        {
            inputs.tabPrev = false;
            Current.OnTabPrev();
        }

        if (inputs.tabNext)
        {
            inputs.tabNext = false;
            Current.OnTabNext();
        }

        if (inputs.advance)
        {
            inputs.advance = false;
            Current.OnAdvance();

            if (!IsOpen)
            {
                return;
            }
        }

        if (inputs.cancel)
        {
            inputs.cancel = false;

            if (!Current.OnCancel())
            {
                Back();
            }
        }
    }

    private void OnPausePressed()
    {
        if (rootScreen != null) return;
        if (Current != null && Current.BlocksShortcuts) return;

        if (IsOpen) CloseAll();
        else Open(pauseScreen);
    }

    private void OnJournalPressed()
    {
        if (rootScreen != null) return;
        if (Current != null && Current.BlocksShortcuts) return;

        if (!IsOpen) Open(bookScreen);
        else if (Current == bookScreen) CloseAll();
    }

    public void Open(MenuScreen screen)
    {
        if (screen == null || Current == screen) return;

        if (Current != null)
        {
            Hide(Current);
        }

        stack.Add(screen);
        Show(screen);
        ApplyState();
    }

    public void Back()
    {
        if (!IsOpen || Current == rootScreen) return;

        Hide(Current);
        stack.RemoveAt(stack.Count - 1);

        if (IsOpen)
        {
            Show(Current);
        }

        ApplyState();
    }

    public void CloseAll()
    {
        if (!IsOpen || Current == rootScreen) return;

        Hide(Current);

        while (IsOpen && Current != rootScreen)
        {
            stack.RemoveAt(stack.Count - 1);
        }

        if (IsOpen)
        {
            Show(Current);
        }

        ApplyState();
    }

    private void Show(MenuScreen screen)
    {
        screen.gameObject.SetActive(true);
        screen.OnShown();
        SelectFirst(screen);
    }

    private void Hide(MenuScreen screen)
    {
        screen.OnHidden();
        screen.gameObject.SetActive(false);
    }

    private static void SelectFirst(MenuScreen screen)
    {
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(screen.GetFirstSelected());
    }

    private void ApplyState()
    {
        bool open = IsOpen;
        bool pauses = false;

        foreach (MenuScreen screen in stack)
        {
            if (screen.PausesGame)
            {
                pauses = true;
            }
        }

        Time.timeScale = pauses ? 0f : 1f;
        AudioListener.pause = pauses;

        if (backdrop != null)
        {
            backdrop.SetActive(open && Current.ShowsBackdrop);
        }

        bool freeCursor = open && Current.ShowsCursor;
        Cursor.lockState = freeCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeCursor;

        if (open != inMenuMode && inputs != null)
        {
            inMenuMode = open;
            inputs.SetMenuMode(open);
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}