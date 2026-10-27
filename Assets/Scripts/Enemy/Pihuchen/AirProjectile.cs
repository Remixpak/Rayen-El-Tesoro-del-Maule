using UnityEngine;

public class AirGustProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f; // Tiempo antes de autodestruirse si no acierta
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // Las ráfagas de aire suelen ir en línea recta sin gravedad
        }
    }

    // Método para inicializar la dirección hacia donde debe viajar
    public void Initialize(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Rotar el proyectil para que mire hacia el jugador
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Asignar velocidad inicial
        rb.linearVelocity = direction * speed;

        // Destruir el proyectil después de un tiempo para liberar memoria
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar colisión con el jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("[AirGust] ¡La ráfaga golpeó al jugador!");
            // Aquí puedes restar vida al jugador si tienes un script de salud
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            // Opcional: destruir al chocar con paredes u otros objetos (excepto el propio enemigo)
            Destroy(gameObject);
        }
    }
}