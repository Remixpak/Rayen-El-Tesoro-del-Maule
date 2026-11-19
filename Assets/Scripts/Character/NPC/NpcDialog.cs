using System;
using UnityEngine;

public class NpcDialog : MonoBehaviour
{
    [SerializeField] public ConversationTemplate conversation;

    private DialogManager dialogManager;

    public event Action OnDialogFinished;

    public ConversationTemplate Conversation => conversation;

    private void OnEnable()
    {
        if (dialogManager == null)
            dialogManager = FindFirstObjectByType<DialogManager>(FindObjectsInactive.Include);

        if (dialogManager != null)
            dialogManager.ConversationEnded += HandleConversationEnded;
    }

    private void OnDisable()
    {
        if (dialogManager != null)
            dialogManager.ConversationEnded -= HandleConversationEnded;
    }

    private void HandleConversationEnded()
{
    Debug.Log($"[NpcDialog] {name} comparando: Current={(dialogManager.CurrentConversation != null ? dialogManager.CurrentConversation.name : "NULL")} vs Mia={(conversation != null ? conversation.name : "NULL")}");

    if (dialogManager.CurrentConversation == conversation)
    {
        OnDialogFinished?.Invoke();
    }
}
}