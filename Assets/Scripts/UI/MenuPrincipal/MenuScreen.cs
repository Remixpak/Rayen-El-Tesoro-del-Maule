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
}