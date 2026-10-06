using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Line
{
    public string speakerName;
    [TextArea(2, 3)]
    public string DialogText;
    public Sprite speakerSprite;
}
[CreateAssetMenu(fileName = "New Conversation Template", menuName = "Dialog/Conversation")]


public class ConversationTemplate : ScriptableObject
{
    public List<Line> conversationLines;
}
