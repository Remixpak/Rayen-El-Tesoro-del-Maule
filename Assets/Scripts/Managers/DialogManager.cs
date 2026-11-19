using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManager : MenuScreen
{
    [Header("Textos de la UI")]
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private UnityEngine.UI.Image speakerImage;

    [Header("Efecto de escritura")]
    [SerializeField, Min(0f)] private float charactersPerSecond = 40f; // velocidad de escritura (0 = el texto aparece de golpe :p)

    [Header("Opciones")]
    [SerializeField] private bool showCursor = false;     // muestra el cursor durante el diálogo
    [SerializeField] private bool dimBackground = false;  // oscurece el resto de la pantalla durante el diálogo

    [Header("Debug")]
    [SerializeField] private ConversationTemplate testConversation;

    private bool isDialogActive = false;
    private bool isTyping = false;                         // true mientras la línea actual se está escribiendo
    private Coroutine typingRoutine;
    private readonly Queue<Line> lines = new Queue<Line>(); // cola de líneas de diálogo para mostrarlas en orden

    public bool IsDialogActive
    {
        get { return isDialogActive; }
    }

    // Para conectar misiones o eventos cuando termina una conversación (también si se cierra con Esc/B)
    public event Action ConversationEnded;

    // Mientras dura el diálogo no se puede abrir la pausa ni el diario
    public override bool BlocksShortcuts
    {
        get { return true; }
    }

    public override bool ShowsCursor
    {
        get { return showCursor; }
    }

    public override bool ShowsBackdrop
    {
        get { return dimBackground; }
    }

    private void Awake()
    {
        if (dialogText == null || speakerNameText == null || speakerImage == null)
        {
            Debug.LogError("DialogManager: faltan referencias de texto o imagen en el Inspector.", this);
        }
    }

    // Lo llama el PlayerInteractor con la conversación del NPC
    public void getConversation(ConversationTemplate conversation)
    {
        if (isDialogActive)
        {
            return;
        }

        lines.Clear();

        if (conversation != null && conversation.conversationLines != null)
        {
            foreach (Line line in conversation.conversationLines)
            {
                lines.Enqueue(line);
            }
        }

        if (lines.Count == 0)
        {
            return;
        }

        if (Manager == null)
        {
            return;
        }

        isDialogActive = true;
        Manager.Open(this); // MenuManager: congela el juego, cambia al mapa UI y activa este objeto
    }

    // Para probar la caja de diálogo sin acercarte a un NPC (menú ⋮ del componente, con Play activo)
    [ContextMenu("Probar diálogo")]
    private void TestDialog()
    {
        getConversation(testConversation);
    }

    // ---- Llamadas del MenuManager ----

    public override void OnShown()
    {
        ProduceNextLine();
    }

    public override void OnAdvance()
    {
        if (!isDialogActive)
        {
            return;
        }

        // si la línea todavía se está escribiendo, la primera pulsación la completa; la siguiente pasa a la próxima línea
        if (isTyping)
        {
            CompleteLine();
            return;
        }

        ProduceNextLine();
    }

    // Se ejecuta al cerrarse la pantalla, ya sea por terminar la conversación o por Esc/B
    public override void OnHidden()
    {
        StopTyping();
        isDialogActive = false;
        lines.Clear();
        ConversationEnded?.Invoke();
    }

    // ---- Interno ----

    private void ProduceNextLine()
    {
        if (lines.Count == 0)
        {
            Manager.Back(); // fin de la conversación: cierra la pantalla y se retoma el juego
            return;
        }

        Line currentLine = lines.Dequeue();
        speakerNameText.text = currentLine.speakerName;
        speakerImage.sprite = currentLine.speakerSprite;
        speakerImage.enabled = currentLine.speakerSprite != null;

        StartTyping(currentLine.DialogText);
    }

    // empieza a escribir la línea (o la muestra de golpe si la velocidad es 0)
    private void StartTyping(string text)
    {
        StopTyping();

        dialogText.text = text;

        if (charactersPerSecond <= 0f)
        {
            dialogText.maxVisibleCharacters = int.MaxValue;
            return;
        }

        dialogText.maxVisibleCharacters = 0;
        typingRoutine = StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        isTyping = true;

        dialogText.ForceMeshUpdate(); // para que textInfo tenga el número real de caracteres visibles
        int total = dialogText.textInfo.characterCount;
        float visible = 0f;

        while (visible < total)
        {
            // tiempo sin escalar: el juego está congelado (timeScale = 0) durante el diálogo
            visible += charactersPerSecond * Time.unscaledDeltaTime;
            dialogText.maxVisibleCharacters = Mathf.Min(Mathf.FloorToInt(visible), total);
            yield return null;
        }

        CompleteLine();
    }

    // muestra la línea entera de golpe
    private void CompleteLine()
    {
        StopTyping();
        dialogText.maxVisibleCharacters = int.MaxValue;
    }

    private void StopTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        isTyping = false;
    }
}