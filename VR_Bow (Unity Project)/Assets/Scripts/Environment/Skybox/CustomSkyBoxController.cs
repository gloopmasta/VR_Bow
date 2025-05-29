using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CustomSkyBoxController : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] Material material;
    [SerializeField] float flashDuration = 0.5f;
    [SerializeField] float flashIntensity = 1f;
    [SerializeField] float colorFadeDuration = 1f;

    [Header("Color Settings")]
    [SerializeField] Color slowTimeColor = Color.blue;
    [SerializeField] Color scoreColor = Color.green;

    [Header("Event Channels")]
    [SerializeField] private SlowTimeSO slowTimeEvents;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;

    private float originalExposure;
    private Color originalTint;
    private CancellationTokenSource flashCancellationTokenSource;
    private CancellationTokenSource colorFadeCancellationTokenSource;
    private bool isSlowTimeActive = false; // Track slow time state

    private void Start()
    {
        if (material != null)
        {
            originalExposure = material.GetFloat("_Exposure");
            originalTint = material.GetColor("_Tint");
        }
    }

    private void OnEnable()
    {
        BeatManager.OnBeatChange += FlashMaterial;
        slowTimeEvents.OnSlowTimeEnter += OnSlowTimeEnter;
        slowTimeEvents.OnSlowTimeExit += OnSlowTimeExit;
        arrowHitEvents.OnArrowHitEnemy += OnArrowHitEnemy;
    }

    private void OnDisable()
    {
        BeatManager.OnBeatChange -= FlashMaterial;
        slowTimeEvents.OnSlowTimeEnter -= OnSlowTimeEnter;
        slowTimeEvents.OnSlowTimeExit -= OnSlowTimeExit;
        arrowHitEvents.OnArrowHitEnemy -= OnArrowHitEnemy;

        // Cancel any ongoing operations
        flashCancellationTokenSource?.Cancel();
        flashCancellationTokenSource?.Dispose();
        colorFadeCancellationTokenSource?.Cancel();
        colorFadeCancellationTokenSource?.Dispose();

        // Reset material to original state
        if (material != null)
        {
            material.SetFloat("_Exposure", originalExposure);
            material.SetColor("_Tint", originalTint);
        }
    }

    private void FlashMaterial()
    {   // Skip flash if slow time is active
        if (material == null || isSlowTimeActive) return;

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

                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            material.SetFloat("_Exposure", originalExposure);
        }
        finally
        {
            if (cancellationToken.IsCancellationRequested && material != null)
            {
                material.SetFloat("_Exposure", originalExposure);
            }
        }
    }

    private void OnSlowTimeEnter(float value)
    {
        isSlowTimeActive = true;
        if (material == null) return;

        // Cancel previous color fade
        colorFadeCancellationTokenSource?.Cancel();
        colorFadeCancellationTokenSource = new CancellationTokenSource();

        FadeTintAsync(slowTimeColor, colorFadeDuration, colorFadeCancellationTokenSource.Token).Forget();
    }

    private void OnSlowTimeExit()
    {
        isSlowTimeActive = false;
        if (material == null) return;

        // Cancel previous color fade
        colorFadeCancellationTokenSource?.Cancel();
        colorFadeCancellationTokenSource = new CancellationTokenSource();

        FadeTintAsync(originalTint, colorFadeDuration, colorFadeCancellationTokenSource.Token).Forget();
    }

    private void OnArrowHitEnemy(int value)
    {
        if (material == null) return;

        // Cancel previous operations
        flashCancellationTokenSource?.Cancel();
        colorFadeCancellationTokenSource?.Cancel();
        colorFadeCancellationTokenSource = new CancellationTokenSource();

        FlashTintAsync(scoreColor, flashDuration, colorFadeCancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid FadeTintAsync(Color targetTint, float duration, CancellationToken cancellationToken)
    {
        Color startTint = material.GetColor("_Tint");

        try
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                material.SetColor("_Tint", Color.Lerp(startTint, targetTint, t));

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            material.SetColor("_Tint", targetTint);
        }
        finally
        {
            if (cancellationToken.IsCancellationRequested && material != null)
            {
                material.SetColor("_Tint", startTint);
            }
        }
    }

    private async UniTaskVoid FlashTintAsync(Color flashTint, float duration, CancellationToken cancellationToken)
    {
        Color startTint = material.GetColor("_Tint");

        try
        {
            // Flash to target color
            float elapsedTime = 0f;
            while (elapsedTime < duration / 2f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2f);
                material.SetColor("_Tint", Color.Lerp(startTint, flashTint, t));

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            // Flash back to original color
            elapsedTime = 0f;
            while (elapsedTime < duration / 2f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (duration / 2f);
                material.SetColor("_Tint", Color.Lerp(flashTint, startTint, t));

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            material.SetColor("_Tint", startTint);
        }
        finally
        {
            if (cancellationToken.IsCancellationRequested && material != null)
            {
                material.SetColor("_Tint", startTint);
            }
        }
    }
}