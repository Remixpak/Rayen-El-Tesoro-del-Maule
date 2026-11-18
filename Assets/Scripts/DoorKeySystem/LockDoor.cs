using UnityEngine;

//logica de puerta que requiere llaves para abrirse
public class LockDoor : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";

    [Header("Físicas")]
    [Tooltip("El collider que bloquea el paso. Si lo dejas vacío, buscará todos los colliders en este objeto y sus hijos.")]
    [SerializeField] private Collider[] doorColliders; 

    private bool isOpen = false;

    private void Start()
    {
        // Si no asignaste colliders en el Inspector, los buscamos automáticamente
        if (doorColliders == null || doorColliders.Length == 0)
        {
            doorColliders = GetComponentsInChildren<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryOpenDoor(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            TryOpenDoor(collision.collider.gameObject);
        }
    }

    private void TryOpenDoor(GameObject playerObject)
    {
        if (isOpen) return;

        KeyInventory playerInventory = playerObject.GetComponentInParent<KeyInventory>();

        if (playerInventory != null && playerInventory.HasKey())
        {
            playerInventory.RemoveKey();
            isOpen = true;
            Debug.Log($"Se uso una llave. Llaves restantes: {playerInventory.KeysAdquired}");

            
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger(openTriggerName);
            }

            foreach (Collider col in doorColliders)
            {
                if (col != null)
                {
                    col.enabled = false; 
                }
            }
        }
    }
}