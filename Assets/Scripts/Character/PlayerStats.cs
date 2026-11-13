using System;
using UnityEngine;

// gestiona la vida, la municion y el oro del jugador
// los valores viven en GameSession.Data (sobreviven entre escenas y se podran guardar) y este script avisa con eventos cuando cambian

//falta el metodo de gastar oro y de implementar bien el recibir daño + la logica de lo que pasa al morir
public class PlayerStats : MonoBehaviour
{
    [Header("Capacidad máxima")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxAmmo = 6; // tambien es el numero de balas que muestra el HUD

    [Header("Valores al empezar una partida nueva")]
    [SerializeField] private int startHealth = 100;
    [SerializeField] private int startAmmo = 6;
    [SerializeField] private int startGold = 0;

    [Header("Premios")]
    [SerializeField] private bool collectPickups = true; // si esta activo, escucha los premios recogidos (Pickup.Collected)

    // los eventos avisan al HUD y a cualquier sistema que necesite saber que algo cambio
    public event Action<int, int> HealthChanged; // vida actual, vida maxima
    public event Action<int, int> AmmoChanged;   // municion actual, municion maxima
    public event Action<int> GoldChanged;        // oro actual
    public event Action Died;

    public int Health
    {
        get { return GameManager.Data.health; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    public int Ammo
    {
        get { return GameManager.Data.ammo; }
    }

    public int MaxAmmo
    {
        get { return maxAmmo; }
    }

    public int Gold
    {
        get { return GameManager.Data.gold; }
    }

    public bool IsDead
    {
        get { return GameManager.Data.health <= 0; }
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        maxAmmo = Mathf.Max(1, maxAmmo);
        startHealth = Mathf.Clamp(startHealth, 1, maxHealth);
        startAmmo = Mathf.Clamp(startAmmo, 0, maxAmmo);
        startGold = Mathf.Max(0, startGold);
    }

    private void Awake()
    {
        GameData data = GameManager.Data;

        if (!data.statsInitialized)
        {
            // al crear una partida nueva reponemos todos los valores iniciales
            data.statsInitialized = true;
            data.health = startHealth;
            data.ammo = startAmmo;
            data.gold = startGold;
        }
        else
        {
            // por si cambiaste los maximos en el Inspector, o si la escena se carga con 0 de vida (o al  morir) se revive con la vida maxima
            data.health = data.health <= 0 ? maxHealth : Mathf.Min(data.health, maxHealth);
            data.ammo = Mathf.Clamp(data.ammo, 0, maxAmmo);
        }
    }

    private void OnEnable()
    {
        if (collectPickups)
        {
            Pickup.Collected += OnPickupCollected;
        }
    }

    private void OnDisable()
    {
        Pickup.Collected -= OnPickupCollected;
    }

    // se llama cuando el jugador recoge un premio
    private void OnPickupCollected(PickupType type, int amount)
    {
        switch (type)
        {
            case PickupType.Gold:
                AddGold(amount);
                break;
            case PickupType.Health:
                Heal(amount);
                break;
            case PickupType.Ammo:
                AddAmmo(amount);
                break;
        }
    }


    // los enemigos llamaran a este metodo para hacer daño al jugador
    public void TakeDamage(int amount)
    {
        GameData data = GameManager.Data;

        if (amount <= 0 || data.health <= 0)
        {
            return;
        }

        data.health = Mathf.Max(0, data.health - amount);
        HealthChanged?.Invoke(data.health, maxHealth);

        if (data.health <= 0)
        {
            Debug.Log("[Player] La vida llegó a 0: el jugador ha muerto.", this);
            Died?.Invoke();
        }
    }
    //metodo de para curarse 
    public void Heal(int amount)
    {
        GameData data = GameManager.Data;

        // un jugador muerto no se cura
        if (amount <= 0 || data.health <= 0)
        {
            return;
        }

        data.health = Mathf.Min(maxHealth, data.health + amount);
        HealthChanged?.Invoke(data.health, maxHealth);
    }

    // 

    // metodo para gastar la municion 
    public bool TryUseAmmo(int amount = 1)
    {
        GameData data = GameManager.Data; // guardamos la referencia para cuando hagamos el guardado

        if (amount <= 0 || data.ammo < amount)
        {
            return false;
        }

        data.ammo -= amount;
        AmmoChanged?.Invoke(data.ammo, maxAmmo);
        return true;
    }

    //metodo para agregar municion 
    public void AddAmmo(int amount)
    {
        GameData data = GameManager.Data;

        if (amount <= 0)
        {
            return;
        }

        data.ammo = Mathf.Min(maxAmmo, data.ammo + amount); // lo que sobra de la capacidad maxima se pierde
        AmmoChanged?.Invoke(data.ammo, maxAmmo);
    }

    // metodo para agregar oro

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        GameManager.Data.gold += amount;
        GoldChanged?.Invoke(GameManager.Data.gold);
    }

    // para la "tienda" que tendremos en un futuro con los santos del mar
    public bool TrySpendGold(int amount)
    {
        GameData data = GameManager.Data;

        if (amount <= 0 || data.gold < amount)
        {
            return false;
        }

        data.gold -= amount;
        GoldChanged?.Invoke(data.gold);
        return true;
    }

    // ---- Pruebas rapidas (menu ⋮ del componente, con Play activo) ---- LUEGO BORRAR

    [ContextMenu("Probar: recibir 20 de daño")]
    private void TestDamage()
    {
        TakeDamage(20);
    }

    [ContextMenu("Probar: curar 20")]
    private void TestHeal()
    {
        Heal(20);
    }

    [ContextMenu("Probar: gastar 1 bala")]
    private void TestUseAmmo()
    {
        TryUseAmmo();
    }

    [ContextMenu("Probar: sumar 10 de oro")]
    private void TestGold()
    {
        AddGold(10);
    }
}