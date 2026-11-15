using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Serialization;

public class InteractPrompt : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private MenuManager menu;
    [SerializeField] private GameObject visual;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private TMP_Text actionLabel;

    [Header("Textos")]
    [FormerlySerializedAs("actionText")]
    [SerializeField] private string talkText = "Hablar";
    [SerializeField] private string openText = "Abrir";
    [SerializeField] private string keyboardKey = "E";
    [SerializeField] private string gamepadKey = "B";
    [SerializeField] private string playStationKey = "O";

    [Header("Posición")]
    [SerializeField] private Vector2 screenOffset = new Vector2(80f, 0f);

    private RectTransform rect;
    private Canvas canvas;
    private Camera cam;
    private GameObject trackedNpc;
    private Collider trackedCollider;
    private InputDevice lastDevice;
    private bool isInitialized;

    private void Start()
    {
        rect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        if (visual != null) visual.SetActive(false);

        TryInitialize();
    }

    private void TryInitialize()
    {
        if (isInitialized) return;

        if (interactor == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            interactor = GameManager.Instance.player.GetComponent<PlayerInteractor>();
        }

        if (interactor == null || visual == null || keyLabel == null || actionLabel == null)
        {
            return;
        }

        isInitialized = true;
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        Canvas.willRenderCanvases += UpdatePosition;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
        Canvas.willRenderCanvases -= UpdatePosition;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            TryInitialize();
            if (!isInitialized) return;
        }

        bool show = interactor.CanInteract && (menu == null || !menu.IsOpen);

        if (show)
        {
            GameObject target = interactor.CurrentTarget;

            if (target != trackedNpc)
            {
                trackedNpc = target;
                trackedCollider = target.GetComponent<Collider>();
            }

            string action = interactor.CurrentIsChest ? openText : talkText;

            if (actionLabel.text != action)
            {
                actionLabel.text = action;
            }

            string label = GetKeyLabel();

            if (keyLabel.text != label)
            {
                keyLabel.text = label;
            }
        }

        if (visual.activeSelf != show)
        {
            visual.SetActive(show);
        }
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        InputAction action = obj as InputAction;
        if (action != null && action.activeControl != null)
        {
            lastDevice = action.activeControl.device;
        }
    }

    private string GetKeyLabel()
    {
        if (lastDevice is DualShockGamepad) return playStationKey;
        if (lastDevice is Gamepad) return gamepadKey;
        return keyboardKey;
    }

    private void UpdatePosition()
    {
        if (visual == null || !visual.activeSelf || trackedNpc == null) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 screenPoint = cam.WorldToScreenPoint(GetAnchorPoint());

        if (screenPoint.z <= 0f)
        {
            rect.position = new Vector3(-10000f, -10000f, 0f);
            return;
        }

        float scale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;

        rect.position = new Vector3(
            screenPoint.x + screenOffset.x * scale,
            screenPoint.y + screenOffset.y * scale,
            0f);
    }

    private Vector3 GetAnchorPoint()
    {
        if (trackedCollider != null)
        {
            Bounds bounds = trackedCollider.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        return trackedNpc.transform.position + Vector3.up * 1.8f;
    }
}