using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// este script representa una fila de la lista del diario: muestra el titulo de la entrada
// y avisa cuando se hace click en ella o cuando el cursor queda encima (con mando o teclado)
[RequireComponent(typeof(Button))]
public class JournalEntrySlot : MonoBehaviour, ISelectHandler
{
    [SerializeField] private TMP_Text titleText; // texto donde se muestra el titulo de la entrada

    public JournalEntry Entry { get; private set; } // entrada del diario que representa esta fila

    private Action<JournalEntrySlot> selected; // callback que se llama cuando el cursor queda encima de esta fila

    // funcion que se llama al crear la fila para configurarla con la entrada, que hacer al hacer click y que hacer al seleccionarla
    public void Setup(JournalEntry entry, Action<JournalEntrySlot> clicked, Action<JournalEntrySlot> selected)
    {
        // si la entrada es null avisamos por consola y no hacemos nada
        if (entry == null)
        {
            Debug.LogError("JournalEntrySlot: la entrada es null.", this);
            return;
        }

        // si falta el texto del titulo avisamos por consola y no hacemos nada
        if (titleText == null)
        {
            Debug.LogError("JournalEntrySlot: falta asignar Title Text en el prefab.", this);
            return;
        }

        Entry = entry;
        this.selected = selected;
        titleText.text = entry.Title; // mostramos el titulo de la entrada

        // quitamos listeners anteriores y añadimos el nuevo (por si se reutiliza la fila)
        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => clicked?.Invoke(this));
    }

    // se llama cuando el cursor (teclado/mando) queda sobre este boton
    public void OnSelect(BaseEventData eventData) => selected?.Invoke(this);
}