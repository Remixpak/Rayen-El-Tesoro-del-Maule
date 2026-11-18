using UnityEngine;

//logica de puerta que requiere llaves para abrise
public class LockDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Usamos CompareTag que es más eficiente que other.tag ==
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
        KeyInventory playerInventory = playerObject.GetComponentInParent<KeyInventory>();

        // Verificamos que no sea null para evitar el NullReferenceException
        if (playerInventory != null && playerInventory.HasKey())
        {
            playerInventory.RemoveKey();
            Debug.Log($"Se Gasto una llave. Llaves restantes: {playerInventory.KeysAdquired}");

            Destroy(gameObject); // Abrimos (destruimos) la puerta
        }
    }
}