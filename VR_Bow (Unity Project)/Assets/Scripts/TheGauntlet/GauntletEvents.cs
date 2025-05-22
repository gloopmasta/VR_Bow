using System;

public static class GauntletEvents
{
    // Event to request spawning an enemy; passes elapsed time to decide difficulty
    public static event Action<float> OnEnemySpawnRequested;

    // Event to request spawning a powerup
    public static event Action OnPowerupSpawnRequested;

    // Event triggered when the jump pad activates
    public static event Action OnJumpPadActivated;

    // Method to invoke the enemy spawn request event
    public static void RequestEnemySpawn(float elapsedTime)
    {
        OnEnemySpawnRequested?.Invoke(elapsedTime);
    }

    // Method to invoke the powerup spawn request event
    public static void RequestPowerupSpawn()
    {
        OnPowerupSpawnRequested?.Invoke();
    }

    // Method to invoke the jump pad activation event
    public static void ActivateJumpPad()
    {
        OnJumpPadActivated?.Invoke();
    }
}
