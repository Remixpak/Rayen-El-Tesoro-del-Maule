using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Configuración")]
    [SerializeField] private EnemyData enemyData; 

    private float currentHealth;
    private Color baseColor;
    private Coroutine hitCoroutine;
    private Material mat;
    private Renderer rend;

    // Propiedad pública por si otros scripts necesitan consultar los datos
    public EnemyData Data => enemyData; 

    private void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"¡El enemigo {name} no tiene asignado un EnemyData!");
            return;
        }

        // Inicializar vida con los datos del ScriptableObject
        currentHealth = enemyData.maxHealth;

        // Configuración del Renderer y Material
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            baseColor = mat.color;
        }
    }

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        currentHealth -= amount;
        Debug.Log($"{enemyData.enemyName} recibió {amount} de daño. Vida: {currentHealth}/{enemyData.maxHealth}");

        // Feedback visual usando los datos del SO
        if (rend != null)
        {
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }
            hitCoroutine = StartCoroutine(HitFeedback());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFeedback()
    {
        mat.color = enemyData.hitColor;
        yield return new WaitForSeconds(enemyData.hitDuration);
        mat.color = baseColor;
        hitCoroutine = null;
    }

    private void Die()
    {
        Debug.Log($"{enemyData.enemyName} ha muerto.");
        Destroy(gameObject);
    }
}