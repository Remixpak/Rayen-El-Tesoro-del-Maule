using System;
using UnityEngine;

[RequireComponent(typeof(NpcDialog))]
public class Novia : MonoBehaviour
{
    [SerializeField] private ConversationTemplate conversationBosqueGuarida;
    [SerializeField] private ConversationTemplate conversationMisionPihuchen;
    [SerializeField] private ConversationTemplate conversationBosqueAccesoATemplo;

    private NpcDialog npcDialog;
    private ConversationTemplate currentConversation;

    public static event Action<ConversationTemplate> OnHabladoConNovia;

    private NpcDialog DialogComponent
    {
        get
        {
            if (npcDialog == null)
                npcDialog = GetComponent<NpcDialog>();
            return npcDialog;
        }
    }

    private void OnEnable()
    {
        DialogComponent.OnDialogFinished += HandleDialogFinished;
        UpdateConversation();
    }

    private void OnDisable()
    {
        if (npcDialog != null)
            npcDialog.OnDialogFinished -= HandleDialogFinished;
    }

    public void UpdateConversation()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.MisionesState)
        {
            case Misiones.guadridaCuatreros:
                currentConversation = conversationBosqueGuarida;
                break;
            case Misiones.BusquedaPihuchen:
                currentConversation = conversationMisionPihuchen;
                break;
            case Misiones.MisionPihuchen:
                currentConversation = conversationBosqueAccesoATemplo;
                break;
            default:
                currentConversation = conversationBosqueGuarida;
                break;
        }

        DialogComponent.conversation = currentConversation;
    }

    private void HandleDialogFinished()
    {
        OnHabladoConNovia?.Invoke(currentConversation);
        UpdateConversation();
    }
}
