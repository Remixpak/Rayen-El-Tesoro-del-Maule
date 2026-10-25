using UnityEngine;

// este script es la base de todas las secciones del libro (diario, mapa, etc.): define lo minimo
// que tiene que tener una seccion (titulo, mostrarse, primer elemento seleccionable y volver)
public abstract class BookSection : MonoBehaviour
{
    [SerializeField] private string title = "Sección"; // titulo de la seccion (se puede cambiar en el inspector)
    public string Title => title; // propiedad de solo lectura para que otros scripts puedan leer el titulo

    // se llama cuando se muestra la seccion 
    public virtual void OnShown()
    {

    }

    // devuelve el primer elemento que se debe seleccionar al mostrar la seccion (por defecto ninguno)
    public virtual GameObject GetFirstSelected() => null;

    // se llama al pulsar cancelar/volver 
    public virtual bool OnCancel() => false;
}