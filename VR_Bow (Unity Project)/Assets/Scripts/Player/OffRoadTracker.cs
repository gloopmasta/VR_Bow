using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class OffRoadTracker : MonoBehaviour
{
    [SerializeField] private float offRoadDuration = 3f;
    [SerializeField] private LayerMask roadLayerMask;

    private CapsuleCollider playerCollider;
    private bool isOffRoad = false;
    private CancellationTokenSource cts;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCollider == null)
        {
            Debug.LogError("CapsuleCollider not found on the player.");
        }
    }

    private void Update()
    {
        if (playerCollider == null) return;

        bool currentlyOnRoad = IsOnRoad();

        if (!currentlyOnRoad && !isOffRoad)
        {
            isOffRoad = true;
            cts = new CancellationTokenSource();
            StartOffRoadCountdown(cts.Token).Forget();
        }
        else if (currentlyOnRoad && isOffRoad)
        {
            isOffRoad = false;
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
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
}
