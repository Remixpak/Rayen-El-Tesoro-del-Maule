using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// cofre: al abrirlo reproduce la animacion y, tras un momento, saca la recompensa desde su centro
public class Chest : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private Animator animator; // si se deja vacio, busca uno en los hijos
    [SerializeField] private string openTrigger = "Open";

    [Header("Recompensa")]
    [SerializeField] private DropTable dropTable; // si se deja vacio, busca uno en este objeto
    [SerializeField] private Transform lootPoint; // punto vacio en el centro del cofre, donde aparece el premio
    [SerializeField] private float dropDelay = 0.5f; // espera para que el premio salga cuando la tapa ya se abrio

    [Header("Eventos")]
    [SerializeField] private UnityEvent onOpened; // para cualquier otra cosa que quieras disparar al abrir

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

        if (dropTable == null)
        {
            dropTable = GetComponent<DropTable>();
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

        onOpened?.Invoke();
        StartCoroutine(DropAfterDelay());
    }

    private IEnumerator DropAfterDelay()
    {
        yield return new WaitForSeconds(dropDelay);
        SpawnLoot();
    }

    // tambien se puede llamar desde un Animation Event del clip, en el frame exacto en que se abre la tapa
    public void SpawnLoot()
    {
        if (dropTable == null)
        {
            Debug.LogWarning($"Chest '{name}': no tiene DropTable asignado.", this);
            return;
        }

        Vector3 position = lootPoint != null ? lootPoint.position : transform.position + Vector3.up * 0.5f;
        dropTable.Drop(position);
    }

    private void OnDrawGizmosSelected()
    {
        if (lootPoint == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lootPoint.position, 0.15f);
    }
}