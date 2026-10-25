using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// este script es la seccion del diario dentro del libro: muestra un indice paginado con las entradas
// y al seleccionar una, muestra su contenido (titulo, cuerpo e imagen) en una vista de detalle
public class JournalSection : BookSection
{
    [Header("Entradas (en el orden del índice)")]
    [SerializeField] private List<JournalEntry> entries = new List<JournalEntry>(); // lista de todas las entradas del diario (en orden)

    [Header("Índice")]
    [SerializeField] private GameObject indexView; // vista donde se ve el indice con las entradas
    [SerializeField] private Transform slotContainer; // objeto con Grid Layout Group donde van las filas
    [SerializeField] private JournalEntrySlot slotPrefab; // prefab de cada fila del indice
    [SerializeField] private int entriesPerPage = 8; // cuantas entradas caben por pagina
    [SerializeField] private Button prevPageButton; // boton para ir a la pagina anterior
    [SerializeField] private Button nextPageButton; // boton para ir a la pagina siguiente
    [SerializeField] private TMP_Text pageLabel; // texto opcional que muestra "1 / 3"

    [Header("Entrada")]
    [SerializeField] private GameObject entryView; // vista donde se ve el detalle de una entrada
    [SerializeField] private TMP_Text entryTitle; // texto del titulo de la entrada abierta
    [SerializeField] private TMP_Text entryBody; // texto del cuerpo de la entrada abierta
    [SerializeField] private Image entryImage; // imagen de la entrada abierta
    [SerializeField] private GameObject sectionTitle; // titulo de la seccion (se oculta cuando se abre una entrada)

    private readonly List<JournalEntry> catalog = new List<JournalEntry>(); // lista de entradas validas (sin nulos)
    private readonly List<JournalEntrySlot> slots = new List<JournalEntrySlot>(); // filas creadas en la pagina actual
    private bool inEntryView; // para saber si estamos viendo el indice o una entrada
    private int pageIndex; // pagina actual del indice
    private JournalEntry lastOpened; // ultima entrada abierta (para recordarla al volver al indice)

    // numero total de paginas (minimo 1)
    private int PageCount =>
        Mathf.Max(1, Mathf.CeilToInt(catalog.Count / (float)Mathf.Max(1, entriesPerPage)));

    private void Awake()
    {
        // conectamos los botones de paginacion con sus funciones
        if (prevPageButton != null)
        {
            prevPageButton.onClick.AddListener(() => ChangePage(-1));
        }
        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(() => ChangePage(1));
        }
    }

    // se llama cuando se muestra la seccion: siempre empieza en el indice
    public override void OnShown() => ShowIndex();

    // devuelve el primer elemento seleccionable de la seccion
    public override GameObject GetFirstSelected()
    {
        if (inEntryView)
        {
            return null; // en la vista de entrada no hay nada que seleccionar
        }

        // resaltamos la ultima entrada leida, si esta en esta pagina
        if (lastOpened != null)
        {
            foreach (JournalEntrySlot s in slots)
            {
                if (s.Entry == lastOpened && s.IsUnlocked)
                {
                    return s.gameObject;
                }
            }
        }

        // si no, la primera entrada desbloqueada de la pagina
        GameObject first = FirstUnlockedSlot();
        if (first != null)
        {
            return first;
        }

        // si no hay entradas, seleccionamos el boton de siguiente pagina (si esta activo)
        if (nextPageButton != null && nextPageButton.gameObject.activeSelf && nextPageButton.interactable)
        {
            return nextPageButton.gameObject;
        }

        return null;
    }

    // se llama al pulsar cancelar/volver: si estamos en una entrada volvemos al indice,
    // si estamos en el indice no lo gestionamos (que lo haga el manager cerrando el libro)
    public override bool OnCancel()
    {
        if (!inEntryView)
        {
            return false; // en el indice no lo gestiona: el manager cierra el libro
        }

        ShowIndex(); // volvemos al indice

        // seleccionamos el primer elemento del indice para poder navegar con mando
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(GetFirstSelected());
        }
        return true; // nosotros hemos gestionado el volver
    }

    // muestra el indice de entradas
    private void ShowIndex()
    {
        inEntryView = false;
        entryView.SetActive(false); // ocultamos la vista de entrada
        indexView.SetActive(true); // mostramos el indice

        if (sectionTitle != null)
        {
            sectionTitle.SetActive(true); // mostramos el titulo de la seccion
        }

        Rebuild(); // reconstruimos las filas de la pagina actual
    }

    // reconstruye las filas del indice para la pagina actual
    private void Rebuild()
    {
        // limpiamos el catalogo y metemos las entradas no nulas
        catalog.Clear();
        foreach (JournalEntry e in entries)
        {
            if (e != null)
            {
                catalog.Add(e);
            }
        }

        // nos aseguramos de que la pagina actual esta dentro de rango
        pageIndex = Mathf.Clamp(pageIndex, 0, PageCount - 1);

        // destruimos las filas anteriores
        foreach (JournalEntrySlot s in slots)
        {
            s.gameObject.SetActive(false); // deja de contar en el layout al instante
            Destroy(s.gameObject);
        }
        slots.Clear();

        // calculamos el rango de entradas que van en esta pagina
        int start = pageIndex * Mathf.Max(1, entriesPerPage);
        int end = Mathf.Min(start + Mathf.Max(1, entriesPerPage), catalog.Count);

        // creamos una fila por cada entrada de la pagina
        for (int i = start; i < end; i++)
        {
            JournalEntry e = catalog[i];
            JournalEntrySlot slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(e, GameSession.IsJournalUnlocked(e), OpenEntry); // le pasamos si esta desbloqueada y que hacer al seleccionarla
            slots.Add(slot);
        }

        // activamos o desactivamos los botones de paginacion segun haga falta
        bool multiplePages = PageCount > 1;
        if (prevPageButton != null)
        {
            prevPageButton.gameObject.SetActive(multiplePages);
            prevPageButton.interactable = pageIndex > 0;
        }
        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(multiplePages);
            nextPageButton.interactable = pageIndex < PageCount - 1;
        }
        if (pageLabel != null)
        {
            pageLabel.gameObject.SetActive(multiplePages);
            pageLabel.text = $"{pageIndex + 1} / {PageCount}"; // mostramos "pagina actual / total"
        }
    }

    // cambia de pagina (direccion = -1 anterior, +1 siguiente)
    private void ChangePage(int direction)
    {
        int target = Mathf.Clamp(pageIndex + direction, 0, PageCount - 1);
        if (target == pageIndex)
        {
            return; // ya estamos en el limite, no hacemos nada
        }

        pageIndex = target;
        Rebuild(); // reconstruimos las filas de la nueva pagina

        // se queda en el boton de pagina para seguir pasando; si ya no se puede, va a la primera entrada
        Button preferred = direction > 0 ? nextPageButton : prevPageButton;
        GameObject next = (preferred != null && preferred.interactable) ? preferred.gameObject : FirstUnlockedSlot();

        // seleccionamos el siguiente elemento
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(next);
        }
    }

    // devuelve la primera fila desbloqueada de la pagina actual
    private GameObject FirstUnlockedSlot()
    {
        foreach (JournalEntrySlot s in slots)
        {
            if (s.IsUnlocked)
            {
                return s.gameObject;
            }
        }
        return null;
    }

    // se llama al seleccionar una entrada del indice: abre su vista de detalle
    private void OpenEntry(JournalEntrySlot slot)
    {
        if (!slot.IsUnlocked)
        {
            return; // si la entrada esta bloqueada no se abre
        }

        lastOpened = slot.Entry; // recordamos cual fue la ultima entrada abierta
        inEntryView = true;

        // rellenamos los textos e imagen de la entrada
        entryTitle.text = slot.Entry.Title;
        entryBody.text = slot.Entry.Body;
        entryImage.sprite = slot.Entry.Image;
        entryImage.enabled = slot.Entry.Image != null; // solo mostramos la imagen si la entrada tiene una

        // ocultamos el indice y mostramos la entrada
        indexView.SetActive(false);
        entryView.SetActive(true);

        if (sectionTitle != null)
        {
            sectionTitle.SetActive(false);
        }

        // en la entrada no hay nada que seleccionar, asi que quitamos la seleccion actual
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // si se desactiva la seccion, nos aseguramos de que el titulo vuelva a estar visible
    private void OnDisable()
    {
        if (sectionTitle != null)
        {
            sectionTitle.SetActive(true);
        }
    }
}