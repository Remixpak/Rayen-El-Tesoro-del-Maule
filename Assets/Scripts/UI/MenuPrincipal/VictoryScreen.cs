using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// pantalla de victoria: un solo boton que carga la escena configurada en el inspector
// hereda de MenuScreen: el MenuManager pausa el juego, bloquea el input de gameplay y libera el cursor
public class VictoryScreen : MenuScreen
{
    [Header("Botón y Escena")]
    [SerializeField] private Button continueButton; // boton para avanzar a la siguiente escena
    [SerializeField] private string targetSceneName; // nombre exacto de la escena a cargar (debe estar en Build Settings)

    // la pantalla de victoria tampoco se cierra con atajos: solo mediante su boton
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
        if (continueButton == null)
        {
            Debug.LogError("VictoryScreen: falta asignar el botón en el Inspector.", this);
            return;
        }

        continueButton.onClick.AddListener(OnContinue);
    }

    private void OnEnable()
    {
        StartCoroutine(RegisterInManager());
    }

    private IEnumerator RegisterInManager()
    {
        yield return null; // espera un frame para que el MenuManager y el input esten inicializados

        if (Manager != null)
        {
            Manager.Open(this);
        }
        else
        {
            Debug.LogError("VictoryScreen: no encuentra el MenuManager. Debe colgar del canvas que lo lleva.", this);
        }
    }

    public override void OnShown()
    {
        SetButtonInteractable(true);
    }

    public override GameObject GetFirstSelected()
    {
        if (continueButton == null)
        {
            return null;
        }

        return continueButton.gameObject;
    }

    // evita cerrar con Esc / B / clic derecho
    public override bool OnCancel()
    {
        return true;
    }

    private void OnContinue()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("VictoryScreen: no se ha especificado el nombre de la escena en 'targetSceneName'.", this);
            return;
        }

        SetButtonInteractable(false); // evita dobles clics mientras cambia de escena

        // Si el juego quedo pausado por MenuManager, restauramos Time.timeScale antes de cambiar de escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(targetSceneName);
    }

    private void SetButtonInteractable(bool value)
    {
        if (continueButton != null)
        {
            continueButton.interactable = value;
        }
    }
}