using NUnit.Framework;
using UnityEngine;

public class Novia : MonoBehaviour
{
    [SerializeField] private ConversationTemplate conversationBosqueGuarida;
    [SerializeField] private ConversationTemplate conversationMisionPihuchen;
    [SerializeField] private ConversationTemplate conversationBosqueAccesoATemplo;
     private NpcDialog npcDialog;
    private ConversationTemplate currentConversation;

    void Start()
    {
        
    }

    public void UpdateConversation()
    {
        switch(GameManager.Instance.MisionesState)
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
        }

        npcDialog = GetComponent<NpcDialog>();
        npcDialog.conversation = currentConversation;
    }

    
}