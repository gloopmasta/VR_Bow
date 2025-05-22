using System.Collections;
using UnityEngine;

public class GauntletManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GauntletConfig config;

    private float elapsedTime = 0f;

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("GauntletConfig is not assigned in GauntletManager!");
            return;
        }

        StartCoroutine(EnemySpawnRoutine());
        StartCoroutine(PowerupSpawnRoutine());
        StartCoroutine(JumpPadRoutine());
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Coroutine that requests enemy spawns. The spawn interval decreases over time based on elapsedTime.
    /// </summary>
    private IEnumerator EnemySpawnRoutine()
    {
        while (true)
        {
            // Calculate the current spawn interval based on elapsed time, interpolating between start and min interval
            float t = Mathf.Clamp01(elapsedTime / config.enemySpawnIntervalDecreaseDuration);
            float currentInterval = Mathf.Lerp(config.enemySpawnIntervalStart, config.enemySpawnIntervalMin, t);

            // Request an enemy spawn and pass the elapsed time to decide difficulty
            GauntletEvents.RequestEnemySpawn(elapsedTime);

            yield return new WaitForSeconds(currentInterval);
        }
    }

    /// <summary>
    /// Coroutine that requests powerup spawns at fixed intervals.
    /// </summary>
    private IEnumerator PowerupSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(config.powerupSpawnInterval);
            GauntletEvents.RequestPowerupSpawn();
        }
    }

    /// <summary>
    /// Coroutine that activates the jump pad at fixed intervals.
    /// </summary>
    private IEnumerator JumpPadRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(config.jumpPadInterval);
            GauntletEvents.ActivateJumpPad();
        }
    }
}
