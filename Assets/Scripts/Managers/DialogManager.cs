using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class DialogManager : MonoBehaviour
{
    //referencias a los textos de la UI
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] TextMeshProUGUI speakerNameText;
    //referencia al canvas completo
    [SerializeField] GameObject dialogCanvas;
    //variables
    bool isDialogActive = false;
    Queue<Line> lines = new Queue<Line>();//cola de lineas de dialogo para mostrarlas en orden
    
    public void getConversation(ConversationTemplate conversation)
    {
        lines.Clear();
        foreach (var line in conversation.conversationLines)
        {
            lines.Enqueue(line);
        }
        isDialogActive = true;
        dialogCanvas.SetActive(true);
        produceNextLine();
    }

    void produceNextLine()
    {
        if (lines.Count == 0)
        {
            isDialogActive = false;
            dialogCanvas.SetActive(false);
            return;
        }
        Line currentLine = lines.Dequeue();
        speakerNameText.text = currentLine.speakerName;
        dialogText.text = currentLine.DialogText;
    }

    private void Update()
    {
        if (isDialogActive && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            produceNextLine();
        }

    }
}

