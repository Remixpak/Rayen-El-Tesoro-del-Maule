using UnityEngine;



//en un futuro quizas es recomendable convertirlo a un ScriptableObject para no re hacer miles de cajas/objetos destruibles con la misma probabilidad sujeta
public class DropTable : MonoBehaviour
{
    [System.Serializable]
    public class DropEntry
    {
        public string label = "Premio";                 // solo para identificar la fila en el Inspector
        public Pickup pickupPrefab;
        [Range(0f, 100f)] public float chance = 25f;    // probabilidad en %, independiente de las demás filas
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [SerializeField] private DropEntry[] drops;

    [Header("Aparición")]
    [SerializeField] private float spawnHeight = 0.5f;      // altura sobre el punto donde se rompe
    [SerializeField] private float scatterRadius = 0.6f;    // reparte los premios en círculo para que no se apilen

    // Tira los dados de cada fila por separado: pueden caer varios premios, uno o ninguno
    public void Drop(Vector3 position)
    {
        if (drops == null)
        {
            return;
        }

        foreach (DropEntry entry in drops)
        {
            if (entry.pickupPrefab == null)
            {
                continue;
            }

            if (Random.value * 100f >= entry.chance)
            {
                continue;
            }

            int amount = Random.Range(entry.minAmount, Mathf.Max(entry.minAmount, entry.maxAmount) + 1);

            Vector2 circle = Random.insideUnitCircle * scatterRadius;
            Vector3 spawnPosition = position + new Vector3(circle.x, spawnHeight, circle.y);

            Pickup pickup = Instantiate(entry.pickupPrefab, spawnPosition, Quaternion.identity);
            pickup.Setup(amount);
        }
    }
}