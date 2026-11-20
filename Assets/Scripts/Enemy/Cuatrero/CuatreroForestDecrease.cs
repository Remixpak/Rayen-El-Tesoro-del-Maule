using UnityEngine;

public class CuatreroForestDecrease : MonoBehaviour
{
    private ForestManager forest;

    private void Start()
    {
        forest = LevelManager.Instance as ForestManager;
    }

    private void OnDestroy()
    {
        ForestManager manager = forest != null ? forest : ForestManager.Instance as ForestManager;
        if (manager != null)
        {
            manager.UnregisterEnemy();
        }
    }
}