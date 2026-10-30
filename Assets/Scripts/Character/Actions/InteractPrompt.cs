using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

// este script muestra el aviso de "Hablar" encima del NPC cuando se puede interactuar con el
// y cambia la letra del boton segun el dispositivo (teclado, mando Xbox, mando PlayStation)
public class InteractPrompt : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInteractor interactor; // referencia al script que detecta el NPC
    [SerializeField] private MenuManager menu; // opcional: oculta el aviso con un menu abierto
    [SerializeField] private GameObject visual; // hijo con el icono y el texto (es lo que se muestra y oculta)
    [SerializeField] private TMP_Text keyLabel; // letra dentro del circulo
    [SerializeField] private TMP_Text actionLabel; // texto "Hablar"

    [Header("Textos")]
    [SerializeField] private string actionText = "Hablar"; // texto que se muestra al lado del boton
    [SerializeField] private string keyboardKey = "E"; // letra que se muestra si se usa teclado
    [SerializeField] private string gamepadKey = "B"; // letra que se muestra si se usa mando Xbox o generico
    [SerializeField] private string playStationKey = "O"; // letra que se muestra si se usa mando PlayStation

    [Header("Posición")]
    [SerializeField] private Vector2 screenOffset = new Vector2(80f, 0f); // en pixeles, respecto a lo alto del NPC

    private RectTransform rect; // rect transform del propio aviso (para moverlo)
    private Canvas canvas; // canvas donde esta el aviso (para el scaleFactor)
    private Camera cam; // camara principal (para pasar de mundo a pantalla)
    private GameObject trackedNpc; // NPC al que sigue el aviso ahora mismo
    private Collider trackedCollider; // collider del NPC (para calcular su punto mas alto)
    private InputDevice lastDevice; // ultimo dispositivo usado (teclado, mando Xbox, mando PS...)

    private void Awake()
    {
        // si falta alguna referencia avisamos por consola y desactivamos el script
        if (interactor == null || visual == null || keyLabel == null || actionLabel == null)
        {
            Debug.LogError("InteractPrompt: faltan referencias en el Inspector.", this);
            enabled = false;
            return;
        }

        rect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        actionLabel.text = actionText; // ponemos el texto de "Hablar" al empezar
        visual.SetActive(false); // el aviso empieza oculto
    }

    private void OnEnable()
    {
        // nos suscribimos a los eventos que necesitamos
        InputSystem.onActionChange += OnActionChange;
        Canvas.willRenderCanvases += UpdatePosition; // se ejecuta justo antes de dibujar, ya con la camara colocada
    }

    private void OnDisable()
    {
        // nos desuscribimos para no dejar referencias colgando
        InputSystem.onActionChange -= OnActionChange;
        Canvas.willRenderCanvases -= UpdatePosition;
    }

    private void Update()
    {
        // el aviso se muestra si se puede interactuar y no hay un menu abierto
        bool show = interactor.CanInteract && (menu == null || !menu.IsOpen);

        if (show)
        {
            GameObject npc = interactor.CurrentNpc;

            // si el NPC ha cambiado, actualizamos referencias
            if (npc != trackedNpc)
            {
                trackedNpc = npc;
                trackedCollider = npc.GetComponent<Collider>();
            }

            // actualizamos la letra del boton segun el dispositivo
            string label = GetKeyLabel();

            if (keyLabel.text != label)
            {
                keyLabel.text = label;
            }
        }

        // mostramos u ocultamos el aviso segun haga falta
        if (visual.activeSelf != show)
        {
            visual.SetActive(show);
        }
    }

    // recuerda con que dispositivo se uso por ultima vez algun control del juego
    private void OnActionChange(object obj, InputActionChange change)
    {
        // solo nos interesan las acciones que se han ejecutado
        if (change != InputActionChange.ActionPerformed)
        {
            return;
        }

        InputAction action = obj as InputAction;

        // guardamos el dispositivo del control que se ha usado
        if (action != null && action.activeControl != null)
        {
            lastDevice = action.activeControl.device;
        }
    }

    // devuelve la letra que toca mostrar segun el dispositivo usado
    private string GetKeyLabel()
    {
        // si el ultimo dispositivo es un mando de PlayStation, mostramos su boton
        if (lastDevice is DualShockGamepad)
        {
            return playStationKey;
        }

        // si es otro tipo de mando (Xbox o generico), mostramos el suyo
        if (lastDevice is Gamepad)
        {
            return gamepadKey;
        }

        // si no, mostramos la tecla de teclado
        return keyboardKey;
    }

    // coloca el aviso encima del NPC (se llama justo antes de dibujar el canvas)
    private void UpdatePosition()
    {
        // si no hay que mostrarlo, no hacemos nada
        if (visual == null || !visual.activeSelf || trackedNpc == null)
        {
            return;
        }

        // si aun no tenemos la camara, la buscamos
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam == null)
        {
            return;
        }

        // pasamos la posicion del NPC a coordenadas de pantalla
        Vector3 screenPoint = cam.WorldToScreenPoint(GetAnchorPoint());

        // si el NPC esta detras de la camara, lo mandamos muy lejos para que no se vea
        if (screenPoint.z <= 0f)
        {
            rect.position = new Vector3(-10000f, -10000f, 0f);
            return;
        }

        // tenemos en cuenta el scaleFactor del canvas para que el offset sea el mismo en cualquier resolucion
        float scale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;

        rect.position = new Vector3(
            screenPoint.x + screenOffset.x * scale,
            screenPoint.y + screenOffset.y * scale,
            0f);
    }

    // calcula el punto de anclaje del aviso: lo mas alto del collider del NPC
    private Vector3 GetAnchorPoint()
    {
        if (trackedCollider != null)
        {
            Bounds bounds = trackedCollider.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        // si no hay collider, usamos la posicion del NPC + 1.8m de altura
        return trackedNpc.transform.position + Vector3.up * 1.8f;
    }
}