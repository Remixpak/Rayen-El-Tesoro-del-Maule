using UnityEngine;

[CreateAssetMenu(fileName = "NewCuatreroData", menuName = "Enemies/Cuatrero Data")]
public class CuatreroData : EnemyData
{
    [Header("Ajustes del Cuatrero")]
    public int maxAmmo = 6;
    public float reloadTime = 3f;
    //public float attackRange = 10f;
    public float safeDistance = 15f; // Distancia a la que huye para recargar
    [Header("Ajustes de Disparo")]
    public float attackRange = 12f;
    public LayerMask hitMask = ~0;
}