using UnityEngine;

public class TutorialDialog : MonoBehaviour
{
    [SerializeField] private ConversationTemplate gamepadDialog;
    [SerializeField] private ConversationTemplate keyboardDialog;

    private NpcDialog npcDialog;

    private void Awake()
    {
        npcDialog = GetComponent<NpcDialog>();
    }

    private void OnEnable()
    {
        // Nos suscribimos para actualizar el diálogo en cuanto el jugador toque mando o teclado
        InputDeviceTracker.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        // Nos desuscribimos al desactivarse o destruirse para evitar fugas de memoria
        InputDeviceTracker.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (npcDialog == null) return;

        bool gamepad = InputDeviceTracker.UsingGamepad;
        npcDialog.conversation = gamepad ? gamepadDialog : keyboardDialog;
    }
}