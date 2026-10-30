using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; protected set; }

    [System.Serializable]
    public class ExitData
    {
        [Header("Configuración de Salida")]
        public string exitName;
        public GameObject triggerObject;
        public string sceneToLoad;

        [Header("Sistema de Spawn")]
        [Tooltip("Un ID único para esta salida (ej: 'DePuebloARio', 'DeBosqueARio')")]
        public string spawnIDToTrigger;

        [Tooltip("Transform o posición exacta donde aparecerá el jugador si llega a esta zona por esta puerta")]
        public Transform spawnPoint;
    }

    [Header("Configuración General de Salidas")]
    [SerializeField] protected ExitData[] mapExits;

    [Header("Spawn por Defecto (Si se entra por primera vez al juego)")]
    [SerializeField] private Transform defaultSpawnPoint;

    protected virtual void Awake()
    {
        Instance = this;
    }

    protected virtual void Start()
    {
        PosicionarJugador();
    }

    public virtual void NotifyExitTriggered(GameObject triggeredObject)
    {
        foreach (var exit in mapExits)
        {
            if (exit.triggerObject == triggeredObject)
            {
                if (!string.IsNullOrEmpty(exit.sceneToLoad))
                {
                    // 1. Guardamos en el GameManager global a qué punto vamos a llegar
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.nextSpawnPointID = exit.spawnIDToTrigger;
                    }

                    // 2. Cargamos la escena
                    Debug.Log($"Cambiando de zona hacia: {exit.sceneToLoad} (Spawn: {exit.spawnIDToTrigger})");
                    SceneManager.LoadScene(exit.sceneToLoad);
                }
                else
                {
                    Debug.LogWarning($"La salida '{exit.exitName}' no tiene una escena asignada.");
                }
                return;
            }
        }
    }

    private void PosicionarJugador()
    {
        Debug.Log("Posicionando al jugador en el spawn correcto...");
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Transform targetSpawn = null;

        // Revisamos si el GameManager tiene un ID guardado
        Debug.Log("ID de spawn a buscar: " + (GameManager.Instance != null ? GameManager.Instance.nextSpawnPointID : "GameManager no encontrado"));
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.nextSpawnPointID))
        {
            string targetID = GameManager.Instance.nextSpawnPointID;
            Debug.Log($"Buscando spawn con ID: {targetID}");
            // Buscamos entre todas las salidas de este mapa cuál coincide con ese ID
            foreach (var exit in mapExits)
            {
                if (exit.spawnIDToTrigger == targetID && exit.spawnPoint != null)
                {
                    targetSpawn = exit.spawnPoint;
                    break;
                }
            }

            // Limpiamos el ID para que no afecte futuros respawns accidentales
            GameManager.Instance.nextSpawnPointID = "";
        }

        // Si no se encontró un spawn específico (por ejemplo, es la primera vez que abres el juego), usamos el por defecto
        if (targetSpawn == null)
        {
            targetSpawn = defaultSpawnPoint;
        }

        // Movemos al jugador a la posición exacta (desactivando el CharacterController temporalmente si usas uno para evitar bugs de físicas)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (targetSpawn != null)
        {
            player.transform.position = targetSpawn.position;
            player.transform.rotation = targetSpawn.rotation;
        }

        if (cc != null) cc.enabled = true;
    }
}