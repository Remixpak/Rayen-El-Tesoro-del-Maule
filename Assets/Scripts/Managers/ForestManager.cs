using System.Collections.Generic;
using UnityEngine;

public class ForestManager : LevelManager
{
    
    private int enemyCount;
    [SerializeField] private List<GameObject> cuatreros;
    [SerializeField] private GameObject criaPihuchen;

    [SerializeField] private GameObject reja;
    

    protected override void Start()
    {
        PosicionarJugador();

        if (GameManager.Instance.MisionesState != Misiones.guadridaCuatreros)
        {
            foreach (GameObject g in cuatreros)
            {
                if (g != null)
                {
                    Destroy(g);
                }
            }
            enemyCount = 0;
            enabled = false;
        }
        else
        {
            enemyCount = FindObjectsByType<Cuatrero>(FindObjectsSortMode.None).Length;
            Debug.Log("Enemigos encontrados al iniciar: " + enemyCount);
            CheckEnemiesCompleted();
        }

        if (GameManager.Instance.MisionesState == Misiones.BusquedaPihuchen && !GameManager.Instance.cria3Destruida)
        {
            criaPihuchen.SetActive(true);
        }

        if (GameManager.Instance.MisionesState == Misiones.MisionPihuchen)
        {
            Destroy(reja);
        }
    }



    // Este método lo llamarán los enemigos cuando mueran
    public void UnregisterEnemy()
    {
        enemyCount--;
        Debug.Log("Enemigos restantes: " + enemyCount);

        CheckEnemiesCompleted();
    }

    private void CheckEnemiesCompleted()
    {
        if (enemyCount <= 0)
        {
            GameManager.Instance.MisionesState = Misiones.BusquedaPihuchen;
            Debug.Log("Cambiando Conversación: Misión busqueda Pihuchen activada.");
            
            // Desactivamos este script para que no siga evaluando
            enabled = false; 
        }
    }
}