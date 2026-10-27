using UnityEngine;

public class PihuchenWing : MonoBehaviour, IDamageable
{
    private int health;

    public void InitWing(int maxHealth)
    {
        health = maxHealth;
        gameObject.SetActive(true); // Asegura que el ala aparezca activa al iniciar/regenerarse
    }

    public bool IsDestroyed()
    {
        return health <= 0 || !gameObject.activeSelf;
    }

    // Este es el método clave que llamará el Raycast del jugador al impactar el ala
    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        if (health <= 0) return;

        health -= Mathf.RoundToInt(amount);
        Debug.Log($"[Ala] Daño recibido por raycast. Salud restante del ala: {health}");

        if (health <= 0)
        {
            health = 0;
            gameObject.SetActive(false); // Desactiva el ala para que el jefe caiga
        }
    }

    // Sobrecarga por si llamas TakeDamage con enteros directamente desde otra parte
    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage, Vector3.zero);
    }
}