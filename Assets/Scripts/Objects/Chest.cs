using UnityEngine;
using UnityEngine.Events;

// plantilla de cofre, falta incoporar la recompensa pero era para unirlo al interactPrompt 
public class Chest : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private Animator animator; // si se deja vacio, busca uno en los hijos
    [SerializeField] private string openTrigger = "Open"; // nombre del trigger del Animator

    [Header("Recompensa")]
    [SerializeField] private UnityEvent onOpened; // aqui se conectara el script de loot desde el Inspector

    public bool IsOpen { get; private set; }

    // true mientras se puede abrir (si ya esta abierto, el aviso deja de salir)
    public bool CanOpen
    {
        get { return !IsOpen; }
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // lo llama PlayerInteractor cuando el jugador pulsa interactuar
    public void Open()
    {
        if (IsOpen) 
        {
            return;
        }

        IsOpen = true;

        if (animator != null)
        {
            animator.SetTrigger(openTrigger);
        }

        Debug.Log($"Cofre abierto: {name}", this); // temporal, hasta conectar el loot
        onOpened?.Invoke();
    }
}