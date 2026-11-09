using UnityEngine;
using UnityEngine.Events;

//este script es para los objetos destruibles dentro de la escena como cajas, vasijas o idk
public class Destructible : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 1f;          // con 1, cualquier golpe lo rompe

    [Header("Al romperse")]
    [SerializeField] private GameObject brokenPrefab;       // versión rota (trozos), opcional para cuando nachito quiera hacer animaciones de esto o dejarlo asi
    [SerializeField] private UnityEvent onBroken;           // para conectar sonidos o partículas

    private float health;
    private bool isBroken;
    private DropTable dropTable;

    private void Awake()
    {
        health = maxHealth;
        dropTable = GetComponent<DropTable>();
    }

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        if (isBroken)
        {
            return;
        }

        health -= amount;

        if (health <= 0f)
        {
            Break();
        }
    }

    private void Break()
    {
        isBroken = true;

        if (brokenPrefab != null)
        {
            Instantiate(brokenPrefab, transform.position, transform.rotation);
        }

        if (dropTable != null)
        {
            dropTable.Drop(transform.position);
        }

        onBroken?.Invoke();
        Destroy(gameObject);
    }
}