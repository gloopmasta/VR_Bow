using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class OffRoadTracker : MonoBehaviour
{
    [SerializeField] private float offRoadDuration = 3f;
    [SerializeField] private LayerMask roadLayerMask;
    [SerializeField] private LevelEventsSO levelEvents;
    [SerializeField] private JumpPadEventSO jumpPadEvent;


    [SerializeField] private bool isEnabled = false;
    private CapsuleCollider playerCollider;
    private bool isOffRoad = false;
    private CancellationTokenSource cts;
    private CancellationTokenSource offroadCTS;
    private CancellationTokenSource fadeCTS;
    private PlayerUIManager uiManager;
    private GameObject warningPanel;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCollider == null)
        {
            Debug.LogError("CapsuleCollider not found on the player.");
        }

        uiManager = GetComponent<PlayerUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("PlayerUIManager not found on the player.");
        }
        else
        {
            warningPanel = uiManager.warningPanel;
        }
    }

    private void OnEnable()
    {
        levelEvents.OnLevelOneStart += () => isEnabled = true; //when starting the level -> enable
        levelEvents.OnLevelOneLose += () => isEnabled = false;
        levelEvents.OnLevelOneWin += () => isEnabled = false;
        levelEvents.OnLevelOneRestart += () => isEnabled = false;
        jumpPadEvent.OnEnterJumpPad += () => isEnabled = false; //entering a jumppad -> disable
    }

    private void Update()
    {
        if (playerCollider == null || uiManager == null || warningPanel == null) return;

        // If disabled, force-cancel all operations and hide the warning
        if (!isEnabled)
        {
            if (isOffRoad || warningPanel.activeSelf) // Only act if the warning was active
            {
                isOffRoad = false;
                offroadCTS?.Cancel();
                offroadCTS?.Dispose();
                offroadCTS = null;

                fadeCTS?.Cancel();
                fadeCTS?.Dispose();
                fadeCTS = new CancellationTokenSource();
                FadeWarningPanelOut(fadeCTS.Token).Forget(); // Force fade-out
            }
            return; // Skip further checks
        }

        bool currentlyOnRoad = IsOnRoad();

        if (!currentlyOnRoad && !isOffRoad)
        {
            isOffRoad = true;

            // Start the 3-second countdown
            offroadCTS = new CancellationTokenSource();
            StartOffRoadCountdown(offroadCTS.Token).Forget();

            // Start fade-in
            fadeCTS?.Cancel();
            fadeCTS = new CancellationTokenSource();
            FadeWarningPanelIn(fadeCTS.Token).Forget();
        }
        else if (currentlyOnRoad && isOffRoad)
        {
            isOffRoad = false;

            // Cancel countdown
            offroadCTS?.Cancel();
            offroadCTS?.Dispose();
            offroadCTS = null;

            // Start fade-out
            fadeCTS?.Cancel();
            fadeCTS = new CancellationTokenSource();
            FadeWarningPanelOut(fadeCTS.Token).Forget();
        }
    }

    private bool IsOnRoad()
    {
        // Define the capsule parameters
        Vector3 point1 = transform.position + Vector3.up * (playerCollider.height / 2 - playerCollider.radius);
        Vector3 point2 = transform.position - Vector3.up * (playerCollider.height / 2 - playerCollider.radius);
        float radius = playerCollider.radius;

        // Check for overlaps with "Road" colliders
        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, roadLayerMask);
        return hits.Length > 0;
    }

    private async UniTaskVoid StartOffRoadCountdown(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(offRoadDuration), cancellationToken: token);
            GetComponent<Player>().Respawn().Forget();
        }
        catch (OperationCanceledException)
        {
            // Countdown was canceled because the player returned to the road
        }
    }

    private async UniTaskVoid FadeWarningPanelIn(CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(1f).AttachExternalCancellation(token); //wait 0.5 seconds before the message
            await uiManager.FadeIn(warningPanel, offRoadDuration).AttachExternalCancellation(token); // You can adjust the fade-in duration
        }
        catch (OperationCanceledException)
        {
            // Fade was canceled
        }
    }

    private async UniTaskVoid FadeWarningPanelOut(CancellationToken token)
    {
        try
        {
            await uiManager.FadeOut(warningPanel, 0.2f).AttachExternalCancellation(token); // You can adjust the fade-out duration
        }
        catch (OperationCanceledException)
        {
            // Fade was canceled
        }
    }
}
