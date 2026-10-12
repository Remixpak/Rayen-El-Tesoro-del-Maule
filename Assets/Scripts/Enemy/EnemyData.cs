using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Información Básica")]
    public string enemyName;
    public GameObject enemyPrefab;

    [Header("Estadísticas de Combate")]
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Movimiento y IA")]
    public float moveSpeed = 3.5f;
    public float chaseRange = 10f;

    [Header("Feedback Visual")]
    public Color hitColor = Color.red;
    public float hitDuration = 0.1f;


}