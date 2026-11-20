using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// pantalla de muerte: dos botones (continuar y salir) que usan el GameManager
// hereda de MenuScreen: el MenuManager se encarga de pausar el juego, bloquear el input de gameplay y liberar el cursor
public class GameOverScreen : MenuScreen
{
    [Header("Botones")]
    [SerializeField] private Button continueButton; // reinicia la escena (GameManager.Reiniciar)
    [SerializeField] private Button quitButton;     // vuelve al menu principal (GameManager.salir)

    // la pantalla de muerte no se puede cerrar con P, Tab, Esc ni B: solo con sus botones
    public override bool BlocksShortcuts
    {
        get { return true; }
    }

    public override bool ShowsBackdrop
    {
        get { return false; }
    }

    private void Awake()
    {
        if (continueButton == null || quitButton == null)
        {
            Debug.LogError("DeathScreen: faltan asignar los botones en el Inspector.", this);
            return;
        }

        continueButton.onClick.AddListener(OnContinue);
        quitButton.onClick.AddListener(OnQuit);
    }

    // si la pantalla se activa a mano (checkbox en el editor o SetActive desde codigo), se registra en el MenuManager para que pause el juego
    // si la abrio el propio MenuManager, Open() detecta que ya es la pantalla actual y no hace nada
    private void OnEnable()
    {
        StartCoroutine(RegisterInManager());
    }

    private IEnumerator RegisterInManager()
    {
        yield return null; // esperamos un frame para que el MenuManager y el input ya esten inicializados

        if (Manager != null)
        {
            Manager.Open(this);
        }
        else
        {
            Debug.LogError("DeathScreen: no encuentra el MenuManager. Debe colgar del canvas que lo lleva.", this);
        }
    }

    public override void OnShown()
    {
        SetButtonsInteractable(true);
    }

    public override GameObject GetFirstSelected()
    {
        if (continueButton == null)
        {
            return null;
        }

        return continueButton.gameObject;
    }

    // Esc / B / clic derecho no hacen nada: devolver true evita que el MenuManager cierre la pantalla
    public override bool OnCancel()
    {
        return true;
    }

    private void OnContinue()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("DeathScreen: no hay GameManager en la escena.", this);
            return;
        }

        SetButtonsInteractable(false); // evita un doble clic mientras se recarga la escena
        GameManager.Instance.Reiniciar();
    }

    private void OnQuit()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("DeathScreen: no hay GameManager en la escena.", this);
            return;
        }

        SetButtonsInteractable(false);
        GameManager.Instance.salir();
    }

    private void SetButtonsInteractable(bool value)
    {
        if (continueButton != null)
        {
            continueButton.interactable = value;
        }

        if (quitButton != null)
        {
            quitButton.interactable = value;
        }
    }
}