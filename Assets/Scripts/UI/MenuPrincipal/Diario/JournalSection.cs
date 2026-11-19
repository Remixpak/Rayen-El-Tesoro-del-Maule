using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// este script es la seccion del diario dentro del libro: muestra una lista de entradas desbloqueadas
// a la izquierda y el detalle de la entrada seleccionada a la derecha
public class JournalSection : BookSection
{
    [Header("Entradas (en el orden de la lista)")]
    [SerializeField] private List<JournalEntry> entries = new List<JournalEntry>(); // lista de todas las entradas del diario (en orden)

    [Header("Lista (izquierda)")]
    [SerializeField] private Transform slotContainer; // objeto Grid con el Vertical Layout Group donde van las filas
    [SerializeField] private JournalEntrySlot slotPrefab; // prefab de cada fila de la lista

    [Header("Detalle (derecha)")]
    [SerializeField] private GameObject entryContent; // agrupa titulo, imagen y cuerpo de la entrada
    [SerializeField] private TMP_Text entryTitle; // texto del titulo de la entrada abierta
    [SerializeField] private TMP_Text entryBody; // texto del cuerpo de la entrada abierta
    [SerializeField] private Image entryImage; // imagen de la entrada abierta

    private readonly List<JournalEntrySlot> slots = new List<JournalEntrySlot>(); // filas creadas en la lista
    private JournalEntry current; // entrada que se esta mostrando ahora mismo
    private bool showingEntry; // para saber si hay una entrada abierta o no

    // al abrir: reconstruye la lista y muestra la primera entrada
    public override void OnShown()
    {
        Rebuild(); // reconstruimos la lista desde cero

        // si hay entradas, mostramos la primera; si no, limpiamos el detalle
        if (slots.Count > 0)
        {
            ShowEntry(slots[0].Entry);
        }
        else
        {
            ClearEntry();
        }
    }

    // el cursor siempre empieza en la primera entrada de la lista
    public override GameObject GetFirstSelected()
    {
        return slots.Count > 0 ? slots[0].gameObject : null;
    }

    // con una entrada abierta, volver deja la derecha en blanco.
    // con la derecha en blanco no lo gestiona: el manager cierra el libro.
    public override bool OnCancel()
    {
        if (!showingEntry)
        {
            return false; // no hay entrada abierta: que lo gestione el manager
        }

        ClearEntry(); // limpiamos el detalle y dejamos la lista visible
        return true; // nosotros hemos gestionado el volver
    }

    // reconstruye la lista de entradas desbloqueadas
    private void Rebuild()
    {
        // destruimos las filas anteriores
        foreach (JournalEntrySlot s in slots)
        {
            s.gameObject.SetActive(false); // deja de contar en el layout al instante
            Destroy(s.gameObject);
        }
        slots.Clear();

        // creamos una fila por cada entrada desbloqueada
        foreach (JournalEntry e in entries)
        {
            // las entradas bloqueadas o nulas no se muestran
            if (e == null || !GameSession.IsJournalUnlocked(e))
            {
                continue;
            }

            JournalEntrySlot slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(e, OnSlotClicked, OnSlotSelected); // le pasamos la entrada y que hacer al hacer click o al seleccionar
            slots.Add(slot);
        }
    }

    // se llama al hacer click en una fila: muestra su detalle
    private void OnSlotClicked(JournalEntrySlot slot) => ShowEntry(slot.Entry);

    // se llama al seleccionar una fila (con mando o raton): muestra su detalle
    private void OnSlotSelected(JournalEntrySlot slot) => ShowEntry(slot.Entry);

    // muestra el detalle de la entrada que le pasamos
    private void ShowEntry(JournalEntry entry)
    {
        // evitamos repetir trabajo si ya se esta mostrando esta entrada
        if (showingEntry && current == entry)
        {
            return;
        }

        current = entry;
        showingEntry = true;

        // rellenamos los textos e imagen de la entrada
        entryTitle.text = entry.Title;
        entryBody.text = entry.Body;
        entryImage.sprite = entry.Image;
        entryImage.enabled = entry.Image != null; // solo mostramos la imagen si la entrada tiene una

        entryContent.SetActive(true); // mostramos el detalle
    }

    // limpia el detalle y deja la derecha en blanco
    private void ClearEntry()
    {
        showingEntry = false;
        current = null;
        entryContent.SetActive(false); // ocultamos el detalle
    }
}