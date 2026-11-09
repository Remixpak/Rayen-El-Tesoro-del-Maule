using System;
using UnityEngine;

public enum PickupType
{
    Gold,
    Health,
    Ammo
}

public class Pickup : MonoBehaviour
{
    [Header("Premio")]
    [SerializeField] private PickupType type = PickupType.Gold;
    [SerializeField] private int amount = 1;                      // cantidad por defecto (el DropTable la sobrescribe)

    [Header("Recogida")]
    [SerializeField] private float collectRadius = 1.2f;          // distancia a la que se recoge
    [SerializeField] private float magnetRadius = 3;           // distancia a la que empieza a ir hacia el jugador
    [SerializeField] private float magnetSpeed = 8f;
    [SerializeField] private float collectDelay = 0.4f;           // espera antes de poder recogerse, para que se vea salir

    [Header("Visual")]
    [SerializeField] private float spinSpeed = 90f;               // grados por segundo (0 = no gira :p)

    // Para conectar vida, oro y munición más adelante: Pickup.Collected += MiMetodo;
    public static event Action<PickupType, int> Collected;

    private static CharacterController playerController;
    private static float nextPlayerSearchTime = 0f;               // Cooldown de búsqueda del jugador
    private const float PlayerSearchCooldown = 1f;                // 1 segundo de espera

    private float spawnTime;
    private bool collected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Collected = null;
        playerController = null;
        nextPlayerSearchTime = 0f;                                // Resetear cooldown
    }

    // Lo llama el DropTable justo después de crear el premio
    public void Setup(int value)
    {
        amount = value;
    }

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (collected || Time.time < spawnTime + collectDelay)
        {
            return;
        }

        if (!FindPlayer())
        {
            return;
        }

        Vector3 target = playerController.bounds.center;

        // Usamos sqrMagnitude para evitar raíces cuadradas en cada frame
        float sqrDistance = (transform.position - target).sqrMagnitude;

        // si estamos muy lejos detenemos la rotacion para no sobrecargar la cpu
        float cullingDistance = magnetRadius * 2f; // la distancia la que deja de girara se calcula el radio de magneticismo multiplicado x 2 
        if (sqrDistance > cullingDistance * cullingDistance)
        {
            return;
        }

        // A partir de aquí el jugador está cerca, así que sí gira
        if (spinSpeed != 0f)
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        // Comprobar recogida
        if (sqrDistance <= collectRadius * collectRadius)
        {
            Collect();
            return;
        }

        // Comprobar magnetismo (directo, atraviesa todo)
        if (sqrDistance <= magnetRadius * magnetRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, magnetSpeed * Time.deltaTime);
        }
    }

    private void Collect()
    {
        collected = true;

        Debug.Log("[Pickup] Recogido: " + amount + " de " + type, this);
        Collected?.Invoke(type, amount);

        Destroy(gameObject);
    }

    private static bool FindPlayer()
    {
        if (playerController != null)
        {
            return true;
        }

        // ponemos un cooldown para no buscar al jugador cada frame 
        if (Time.time < nextPlayerSearchTime)
        {
            return false;
        }

        // marcamos el tiempo para el proximo intento
        nextPlayerSearchTime = Time.time + PlayerSearchCooldown;

        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();

        if (movement == null)
        {
            return false;
        }

        playerController = movement.GetComponent<CharacterController>();
        return playerController != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);

        // Gizmo extra para ver el radio de culling
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f); // Rojo transparente
        Gizmos.DrawWireSphere(transform.position, magnetRadius * 2f);
    }
}