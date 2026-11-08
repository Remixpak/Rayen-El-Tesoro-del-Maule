using UnityEngine;

public class NpcDialog : MonoBehaviour
{
    [SerializeField] private ConversationTemplate conversation;

    //script para poder manejar los estados del juego y las conversaciones (en caso de hacer que se desbloqueen a futuro despues de ciertos sucesos mas dialogos quizas :p)
    public ConversationTemplate Conversation
    {
        get { return conversation; }
    }
}