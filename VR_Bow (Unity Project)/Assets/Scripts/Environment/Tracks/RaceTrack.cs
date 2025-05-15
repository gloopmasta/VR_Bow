using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using PandaBT.Examples.PlayTag;
using TMPro;
using UnityEngine;

public class RaceTrack : MonoBehaviour
{
    [SerializeField] private float raceTime;
    [SerializeField] Material trackMaterial;
    public PlayerUIManager playerUI;

    //async task cancellation
    private UniTaskCompletionSource finishLineReachedSource;
    private CancellationTokenSource cts;


    private void OnEnable()
    {
        DeactivateAllPowerups();
    }
    public async void Activate()
    {
        ActivateAllPowerups();

        
        //enable the player race panel
        playerUI.racePanel.SetActive(true);

        cts = new CancellationTokenSource();
        finishLineReachedSource = new UniTaskCompletionSource();

        var timerTask = CountdownTimerAsync((timeStr) =>
        {
            playerUI.timer.text = timeStr;
        }, cts.Token);

        var finishTask = finishLineReachedSource.Task;

        var (winnerIndex, _) = await UniTask.WhenAny(timerTask, finishTask);

        // Cancel the other one
        cts.Cancel();

        // Clean up
        playerUI.racePanel.SetActive(false);

        DeactivateAllPowerups();
        // Any other cleanup here

        gameObject.SetActive(false);

        //destroy itself
        Destroy(gameObject, 0.2f);
    }

    private void ActivateAllPowerups()
    {
        Powerup[] powerups = GetComponentsInChildren<Powerup>(includeInactive: true);

        foreach (var powerup in powerups)
        {
            powerup.gameObject.SetActive(true);

        }
    }

    public void DeactivateAllPowerups()
    {
        Powerup[] powerups = GetComponentsInChildren<Powerup>(includeInactive: true);

        foreach (var powerup in powerups)
        {
            powerup.gameObject.SetActive(false);
        }
    }

    public async UniTask<bool> CountdownTimerAsync(Action<string> onTimeUpdate, CancellationToken cancellationToken = default)
    {
        float timer = raceTime;

        try
        {
            while (timer > 0f)
            {
                // Format: seconds:milliseconds (e.g., 3:420)
                int seconds = Mathf.FloorToInt(timer);
                int milliseconds = Mathf.FloorToInt((timer - seconds) * 1000f);
                string formattedTime = $"{seconds}:{milliseconds:D3}";

                onTimeUpdate?.Invoke(formattedTime);

                await UniTask.Yield(cancellationToken); // Wait for next frame
                timer -= Time.deltaTime;
            }

            // Final update to 0:000
            onTimeUpdate?.Invoke("0:000");
            return true;
        }
        catch (OperationCanceledException)
        {
            onTimeUpdate?.Invoke("Canceled");
            return false;
        }
    }

    public void OnPlayerReachedFinish()
    {
        if (finishLineReachedSource != null && !finishLineReachedSource.Task.Status.IsCompleted())
        {
            finishLineReachedSource.TrySetResult();
        }
    }



    private void ChangeTrackMaterial()
    {
        Material newTrackmaterial = trackMaterial;

    }

    
}
