using UnityEngine;

// Logica de puerta que requiere llaves para abrirse
public class LockDoor : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";

    private bool isOpen = false;
        
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
        // Si ya está abierta, no hacemos nada
        if (isOpen) return;

        // Buscamos el inventario en el jugador o en sus hijos
        KeyInventory playerInventory = playerObject.GetComponentInParent<KeyInventory>();

        if (playerInventory != null && playerInventory.HasKey())
        {
            playerInventory.RemoveKey();
            isOpen = true;

            // Ejecutamos la animación
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger(openTriggerName);
            }

            Collider[] collidersDeLaPuerta = GetComponentsInChildren<Collider>();
            foreach (Collider col in collidersDeLaPuerta)
            {
                if (!col.isTrigger)
                {
                    col.enabled = false;
                }
            }
        }
    }
}