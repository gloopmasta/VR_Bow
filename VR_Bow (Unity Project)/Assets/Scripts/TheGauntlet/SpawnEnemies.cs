using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    [SerializeField] private GauntletConfig config;
    [SerializeField] private Transform[] spawnPoints;

    private void OnEnable()
    {
        GauntletEvents.OnEnemySpawnRequested += HandleEnemySpawn;
    }

    private void OnDisable()
    {
        GauntletEvents.OnEnemySpawnRequested -= HandleEnemySpawn;
    }

    /// <summary>
    /// Handles spawning an enemy based on elapsed time and difficulty thresholds.
    /// </summary>
    private void HandleEnemySpawn(float elapsedTime)
    {
        if (config == null || config.enemyLibrary == null)
        {
            Debug.LogWarning("EnemySpawner: Missing config or enemy library.");
            return;
        }

        GameObject[] pool = SelectEnemyPool(elapsedTime);
        if (pool == null || pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }

    /// <summary>
    /// Selects the appropriate pool of enemies based on elapsed time.
    /// </summary>
    private GameObject[] SelectEnemyPool(float elapsedTime)
    {
        if (elapsedTime >= config.timeUntilHardEnemies)
        {
            // Mix of easy, medium, and hard
            return config.enemyLibrary.GetRandomMixedPool(0.3f, 0.4f, 0.3f);
        }
        else if (elapsedTime >= config.timeUntilMediumEnemies)
        {
            // Mix of easy and medium
            return config.enemyLibrary.GetRandomMixedPool(0.6f, 0.4f, 0f);
        }
        else
        {
            // Only easy enemies
            return config.enemyLibrary.easyEnemies;
        }
    }
}
