using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CustomSkyBoxController : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] float flashDuration = 0.5f;
    [SerializeField] float flashIntensity = 1f;

    private float originalExposure;
    private CancellationTokenSource flashCancellationTokenSource;

    private void Start()
    {
        if (material != null)
        {
            originalExposure = material.GetFloat("_Exposure");
        }
    }

    private void OnEnable()
    {
        BeatManager.OnBeatChange += FlashMaterial;
    }

    private void OnDisable()
    {
        BeatManager.OnBeatChange -= FlashMaterial;

        // Cancel any ongoing flash task
        flashCancellationTokenSource?.Cancel();
        flashCancellationTokenSource?.Dispose();

        // Reset material to original state
        if (material != null)
        {
            material.SetFloat("_Exposure", originalExposure);
        }
    }

    private void FlashMaterial()
    {
        if (material == null) return;

        // Cancel previous flash if it's still running
        flashCancellationTokenSource?.Cancel();
        flashCancellationTokenSource = new CancellationTokenSource();

        FlashAsync(flashCancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid FlashAsync(CancellationToken cancellationToken)
    {
        // Set initial flashed value
        float targetExposure = originalExposure + flashIntensity;
        material.SetFloat("_Exposure", targetExposure);

        try
        {
            float elapsedTime = 0f;
            while (elapsedTime < flashDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / flashDuration;
                float currentExposure = Mathf.Lerp(targetExposure, originalExposure, t);
                material.SetFloat("_Exposure", currentExposure);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                // Check if cancellation was requested after each frame
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }

            // Ensure exact original value at the end
            material.SetFloat("_Exposure", originalExposure);
        }
        finally
        {
            // Clean up if we exited early
            if (cancellationToken.IsCancellationRequested && material != null)
            {
                material.SetFloat("_Exposure", originalExposure);
            }
        }
    }
}
