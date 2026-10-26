using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// este script representa una fila del indice del diario: muestra el titulo de la entrada o "???"
// si esta bloqueada, y avisa al hacer click para abrirla (solo si esta desbloqueada)

//usar la funcion de IsUnlocked y este script para en el futuro hacer los desbloqueos de informacion :p
[RequireComponent(typeof(Button))]
public class JournalEntrySlot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private TMP_Text titleText; // texto donde se muestra el titulo de la entrada (o "???")
    [SerializeField] private string lockedLabel = "???"; // texto que se muestra cuando la entrada esta bloqueada

    public JournalEntry Entry { get; private set; } // entrada del diario que representa esta fila
    public bool IsUnlocked { get; private set; } // si la entrada esta desbloqueada o no

    // funcion que se llama al crear la fila para configurarla con la entrada, si esta desbloqueada y que hacer al hacer click
    public void Setup(JournalEntry entry, bool unlocked, Action<JournalEntrySlot> clicked)
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

        Button button = GetComponent<Button>(); // cogemos el boton del mismo GameObject
        Entry = entry;
        IsUnlocked = unlocked;

        // mostramos el titulo real o el texto de "???" segun este desbloqueada
        titleText.text = unlocked ? entry.Title : lockedLabel;

        // si esta bloqueada, el boton no se puede pulsar y el mando la salta
        button.interactable = unlocked;

        // quitamos listeners anteriores y añadimos el nuevo (por si se reutiliza la fila)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => clicked?.Invoke(this));
    }

    // con raton, pasar por encima selecciona la fila (solo si se puede abrir)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsUnlocked)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}