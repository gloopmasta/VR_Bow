using UnityEngine;

[CreateAssetMenu(menuName = "Gauntlet/GauntletConfig")]
public class GauntletConfig : ScriptableObject
{
    [Header("Enemy Spawn Settings")]
    [Tooltip("Initial time interval between enemy spawns (seconds)")]
    public float enemySpawnIntervalStart = 10f;

    [Tooltip("Minimum time interval between enemy spawns (seconds)")]
    public float enemySpawnIntervalMin = 2f;

    [Tooltip("Duration over which the enemy spawn interval decreases to minimum (seconds)")]
    public float enemySpawnIntervalDecreaseDuration = 120f;

    [Header("Powerup Spawn Settings")]
    [Tooltip("Time interval between powerup spawns (seconds)")]
    public float powerupSpawnInterval = 15f;

    [Header("Jump Pad Settings")]
    [Tooltip("Time interval between jump pad activations (seconds)")]
    public float jumpPadInterval = 25f;

    [Header("Difficulty Timings")]
    [Tooltip("Time after which medium enemies start spawning (seconds)")]
    public float timeUntilMediumEnemies = 30f;

    [Tooltip("Time after which hard enemies start spawning (seconds)")]
    public float timeUntilHardEnemies = 60f;

    [Header("References")]
    [Tooltip("Reference to the enemy library ScriptableObject")]
    public EnemyLibrary enemyLibrary;

    [Tooltip("Reference to the powerup library ScriptableObject")]
    public PowerUpLibrary powerUpLibrary;
}
