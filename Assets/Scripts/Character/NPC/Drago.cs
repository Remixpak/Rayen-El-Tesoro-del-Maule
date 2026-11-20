using UnityEngine;

public class Drago : MonoBehaviour
{

    private NpcDialog dialog;
    [SerializeField] private ConversationTemplate conversation1;
    [SerializeField] private ConversationTemplate conversation2;
    [SerializeField] private ConversationTemplate conversation3;

    private ConversationTemplate currentConversation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialog = GetComponent<NpcDialog>();
        UpdateConversation();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateConversation()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.MisionesState)
        {
            case Misiones.sinMision:
                currentConversation = conversation1;
                break;
            case Misiones.guadridaCuatreros:
                currentConversation = conversation2;
                break;
            default:
                currentConversation = conversation3;
                break;
        }

        dialog.conversation = currentConversation;
    }
}
