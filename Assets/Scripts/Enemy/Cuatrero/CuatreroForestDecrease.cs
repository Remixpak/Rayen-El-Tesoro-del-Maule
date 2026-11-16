using UnityEngine;

public class CuatreroForestDecrease : MonoBehaviour
{
    ForestManager forest;
    void Start()
    {
         forest = LevelManager.Instance as ForestManager;   
    }

    private void OnDestroy()
    {
        // Si el juego se está cerrando, ForestManager puede destruirse antes; validamos que exista
        if (ForestManager.Instance != null)
        {
            forest.UnregisterEnemy();
        }
    }
}
