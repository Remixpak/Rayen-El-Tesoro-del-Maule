using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// este script es la pantalla del libro: contiene varias secciones (diario, mapa, etc.)
// y permite cambiar entre ellas con los botones de pestaña anterior/siguiente
public class BookScreen : MenuScreen
{
    [SerializeField] private BookSection[] sections; // secciones del libro (0 = Diario, 1 = Mapa, etc.)
    [SerializeField] private TMP_Text sectionTitle; // texto que muestra el titulo de la seccion actual

    private int current; // indice de la seccion actual

    // se llama cuando se muestra la pantalla: siempre abre en la primera seccion (el diario)
    public override void OnShown()
    {
        ShowSection(0);
    }

    // se llama al pulsar la pestaña anterior
    public override void OnTabPrev()
    {
        ShowSection(current - 1);
    }

    // se llama al pulsar la pestaña siguiente
    public override void OnTabNext()
    {
        ShowSection(current + 1);
    }

    // se llama al pulsar cancelar/volver: le pasamos el aviso a la seccion actual
    // (si la seccion lo gestiona devuelve true; si no, el manager cierra el libro)
    public override bool OnCancel() =>
        sections != null && sections.Length > 0 && sections[current].OnCancel();

    // devuelve el primer elemento seleccionable de la seccion actual
    public override GameObject GetFirstSelected() =>
        (sections != null && sections.Length > 0) ? sections[current].GetFirstSelected() : null;

    // muestra la seccion que le pasamos por indice (y oculta las demas)
    private void ShowSection(int index)
    {
        // si no hay secciones asignadas no hacemos nada
        if (sections == null || sections.Length == 0)
        {
            return;
        }

        int next = (index + sections.Length) % sections.Length; // da la vuelta al llegar al final

        // ocultamos todas las secciones menos la nueva
        for (int i = 0; i < sections.Length; i++)
        {
            if (i != next)
            {
                sections[i].gameObject.SetActive(false);
            }
        }

        // activamos la seccion nueva y avisamos de que se muestra
        current = next;
        sections[current].gameObject.SetActive(true);
        sections[current].OnShown();

        // actualizamos el titulo de arriba con el de la seccion actual
        if (sectionTitle != null)
        {
            sectionTitle.text = sections[current].Title;
        }

        // seleccionamos el primer elemento de la seccion para poder navegar con mando
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(sections[current].GetFirstSelected());
        }
    }
}