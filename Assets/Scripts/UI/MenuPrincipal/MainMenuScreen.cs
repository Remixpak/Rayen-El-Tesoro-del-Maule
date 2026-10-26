using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//por mientras usaremos este script de menu principal, en un futuro sera conveniente si se mantiene el uso de este agregar una ventana de confirmacion de que si se quiere hacer una nueva partida o si esta seguro el jugador

public class MainMenuScreen : MenuScreen
{
    [Header("Botones")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Escenas")]
    [SerializeField] private string gameScene = "";

    [Header("Confirmación de salir")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Button confirmYes;
    [SerializeField] private Button confirmNo;

    [Header("Mensaje: no hay partida")]
    [SerializeField] private GameObject noGamePanel;
    [SerializeField] private Button noGameAcceptButton;

    [Header("Opciones")]
    [SerializeField] private bool hideButtonsWhilePanelOpen = true; // oculta los 3 botones mientras hay un panel abierto, para que no se solapen

    private void Awake()
    {
        playButton.onClick.AddListener(Play);
        continueButton.onClick.AddListener(Continue);
        quitButton.onClick.AddListener(ShowQuitConfirm);

        SelectButton(continueButton);

        if (confirmPanel == null || confirmYes == null || confirmNo == null || noGamePanel == null || noGameAcceptButton == null)
        {
            Debug.LogError("MainMenuScreen: faltan referencias de Confirm Panel o de No Game Panel en el Inspector.", this);
            return;
        }

        confirmYes.onClick.AddListener(Quit);
        confirmNo.onClick.AddListener(HideQuitConfirm);
        noGameAcceptButton.onClick.AddListener(HideNoGameMessage);
    }

    private void Start()
    {
        SelectButton(continueButton); // por si el EventSystem aún no existía durante Awake
    }

    public override void OnShown()
    {
        SetMenuButtonsVisible(true);

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (noGamePanel != null)
        {
            noGamePanel.SetActive(false);
        }
    }

    public override GameObject GetFirstSelected()
    {
        // Si hay un panel abierto, la selección debe estar en él y no en un botón oculto
        if (confirmPanel != null && confirmPanel.activeSelf)
        {
            return confirmNo.gameObject;
        }

        if (noGamePanel != null && noGamePanel.activeSelf)
        {
            return noGameAcceptButton.gameObject;
        }

        return continueButton.gameObject;
    }

    // Esc / B / clic derecho cierran el panel abierto. Sin panel abierto no hacen nada (es la pantalla raíz)
    public override bool OnCancel()
    {
        if (confirmPanel != null && confirmPanel.activeSelf)
        {
            HideQuitConfirm();
            return true;
        }

        if (noGamePanel != null && noGamePanel.activeSelf)
        {
            HideNoGameMessage();
            return true;
        }

        return false;
    }

    // Jugar

    private void Play()
    {
        if (string.IsNullOrEmpty(gameScene))
        {
            Debug.LogError("MainMenuScreen: falta escribir el nombre de la escena del juego.", this);
            return;
        }

        GameSession.NewGame(); // partida nueva = borra lo desbloqueado (eliminar luego)
        SceneManager.LoadScene(gameScene);
    }

    // Continuar 

    private void Continue()
    {
        if (!HasSavedGame())
        {
            ShowNoGameMessage();
            return;
        }

        Debug.Log("Aún sin función");
    }

    // Por ahora no existe el guardado: siempre devuelve false para que aparezca el cuadrado.
    // Cuando haya sistema de guardado, se reemplaza el contenido de este método.
    private bool HasSavedGame()
    {
        return false;
    }

    private void ShowNoGameMessage()
    {
        if (hideButtonsWhilePanelOpen)
        {
            SetMenuButtonsVisible(false);
        }

        noGamePanel.SetActive(true);
        SelectButton(noGameAcceptButton);
    }

    private void HideNoGameMessage()
    {
        noGamePanel.SetActive(false);
        SetMenuButtonsVisible(true);
        SelectButton(continueButton);
    }

    // Salir

    private void ShowQuitConfirm()
    {
        if (hideButtonsWhilePanelOpen)
        {
            SetMenuButtonsVisible(false);
        }

        confirmPanel.SetActive(true);
        SelectButton(confirmNo); // por defecto es un No
    }

    private void HideQuitConfirm()
    {
        confirmPanel.SetActive(false);
        SetMenuButtonsVisible(true);
        SelectButton(quitButton);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private void SetMenuButtonsVisible(bool visible)
    {
        playButton.gameObject.SetActive(visible);
        continueButton.gameObject.SetActive(visible);
        quitButton.gameObject.SetActive(visible);
    }

    private void SelectButton(Button button)
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}