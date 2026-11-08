using UnityEngine;

public abstract class MenuScreen : MonoBehaviour
{
    [SerializeField] private bool pausesGame = true; // el menú principal lo desmarca
    public bool PausesGame => pausesGame;

    private MenuManager manager;
    protected MenuManager Manager
    {
        get
        {
            if (manager == null) manager = GetComponentInParent<MenuManager>(true);
            return manager;
        }
    }

    public virtual void OnShown() { }
    public virtual void OnHidden() { }

    // Qué elemento recibe la selección del mando al mostrarse
    public virtual GameObject GetFirstSelected() => null;

    // Botón volver: true = la pantalla lo gestionó (ej. del detalle al índice); false = el manager retrocede
    public virtual bool OnCancel() => false;

    public virtual void OnTabPrev() { }
    public virtual void OnTabNext() { }


    //metodos para el showdialog (reciclamos la logica del menu congelando el tiempo directamente desde aca)

    // Se llama cuando el jugador pulsa el botón de avanzar (Espacio / Enter / A / clic)
    public virtual void OnAdvance()
    {
    }

    // Si es true, la pausa (P/Start) y el diario (Tab/Select) no funcionan mientras esta pantalla esté abierta
    public virtual bool BlocksShortcuts
    {
        get { return false; }
    }

    // Si es false, el cursor queda oculto y bloqueado mientras esta pantalla esté abierta
    public virtual bool ShowsCursor
    {
        get { return true; }
    }

    //decidimos si el menu quiere o no tener un fondo oscuro 
    public virtual bool ShowsBackdrop
    {
        get { return true; }
    }
}