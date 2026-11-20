using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using  System.Collections.Generic;
public enum TimeofDay
{
    Day,
    Night
}

public enum Misiones
{
    sinMision,
    guadridaCuatreros,
    BusquedaPihuchen,
    MisionPihuchen
}
public class GameManager : MonoBehaviour
{
    /*
     el game manager es el que se encarga de gestionar gran parte del juego,
     reconoce la escena, a cual debe pasar etc. 
    controla variables globales como el oro o eventos completados
    por ejemplo cuando se haya terminado el evento del toro o matado a una bestia la instancia del game manager lo sabe.
    De ese modo si necesitamos comprobar algo hacemos GameManager.Instance.("nombre del evento") y nos devuelve true o false.
    eso a grandes rasgos
     */
    public static GameManager Instance { get; private set; }
    [HideInInspector] public string nextSpawnPointID = "";

    public static event Action<Misiones> OnMisionActualizada;
    //variables de eventos

    private bool NgurifiluCompleted { get; set; }
    private bool PihuchenCompleted { get; set; }
    private bool JhonElderCompleted { get; set; }

    public GameObject player;

    public TimeofDay currentTimeOfDay { get; set; }
    //a�adir m�s seg�n lo planificado pq ahora no me acuerdo, pero aqui se hace

    //variables de control
    private string currentSceneName { get; set; }
    /*
     cada vez que se cargue una escena se actualiza el nombre de la escena actual, de ese modo podemos saber en que escena estamos y que hacer en consecuencia.
    debe hacerse en el respectivo manager de la escena.
    podemos usarla para saber en que parte dibujar la ubicaci�n del jugador en el mapa, o para saber que eventos mostrar en la escena.
     */

    public static GameData Data => GameSession.Data; // accedemos a los datos de la partida desde el gamemanager


    //ya ahi hice bien la relacion entre el gamemanager y el gamedata donde ahora el gamemanger es el portero que da acceso a los datos hacia player stats por ejemplo
    //si no es lo que querias no me reti xfa 
    //atributos del jugador
    public static int Gold => Data.gold;
    public static int Health => Data.health;
    public static int Ammo => Data.ammo;

    public  WeaponType weapon = Data.WEquiped;

    #region Misiones
    public Misiones MisionesState = Misiones.guadridaCuatreros;
    private int criasRestantes = 3;

    public bool cria1Destruida;
    public bool cria2Destruida;
    public bool cria3Destruida;
    #endregion
    #region  Entradas de diario
    private readonly HashSet<string> entradasDiarioDesbloqueadas = new HashSet<string>();

    public void DesbloquearEntradaDiario(string id)
    {
        Debug.Log("Se llamo a desbloquear entrada");
        if (string.IsNullOrEmpty(id)) return;

        if (entradasDiarioDesbloqueadas.Add(id))
        {
            Debug.Log($"[GameManager] Entrada de diario desbloqueada: {id}");
        }
    }

    public bool EstaEntradaDesbloqueada(JournalEntry entry)
    {
        if (entry == null) return false;
        return entry.UnlockedFromStart || entradasDiarioDesbloqueadas.Contains(entry.Id);
    }
    #endregion


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        player = GameObject.FindWithTag("Player");

        if(player == null)
        {
            Debug.LogWarning("No hay player en el manager");
        }
        
    }
    // Añade esto dentro de GameManager.cs
    public void RegisterPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        
        // Buscamos al nuevo jugador en la escena actual
        player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("No se encontró ningún objeto con la etiqueta 'Player' en la escena: " + scene.name);
        }
    }

    public void UpdateCriasPihuchen()
    {
        Debug.Log("Actualizando crias");
        criasRestantes--;
        if(criasRestantes == 0)
        {
            MisionesState = Misiones.MisionPihuchen;
        }
    }

    public void ActualizarEstadoMision(Misiones nuevoEstado)
    {
        MisionesState = nuevoEstado;
        OnMisionActualizada?.Invoke(MisionesState);
    }

    public void Reiniciar()
    {
        int targetHealth = 100;
        int targetAmmo = 6;

        if (player != null && player.TryGetComponent(out PlayerStats stats))
        {
            targetHealth = stats.MaxHealth;
            targetAmmo = stats.MaxAmmo;
        }

        if (Data != null)
        {
            Data.health = targetHealth;
            Data.ammo = targetAmmo;
        }

        string name = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(name);
        Time.timeScale = 1f;

    }
    public void salir()
    {
        SceneManager.LoadScene("PrincipalPlaceHolder");
        Time.timeScale = 1f; 

    }
}