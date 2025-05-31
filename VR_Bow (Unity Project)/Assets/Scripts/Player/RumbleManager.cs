using UnityEngine;
using UnityEngine.XR;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

public class RumbleManager : MonoBehaviour
{
    [SerializeField] private ArrowHitEventsSO arrowevents;
    [SerializeField] private bool isEngineRumbling = false;
    private InputDevice targetDevice;
    private CancellationTokenSource rumbleTokenSource;

    private void OnEnable()
    {
        rumbleTokenSource = new CancellationTokenSource();
        WaitForController().Forget();

        arrowevents.OnArrowHitEnemy += (int thisDoesNothing) => { RumbleBurst(1f, 2f).Forget(); };
    }

    private async UniTaskVoid WaitForController()
    {
        // Wait until a valid right-hand device is available
        while (!targetDevice.isValid)
        {
            targetDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            await UniTask.Delay(100); // wait 100ms
        }

        if (targetDevice.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
        {
            Debug.Log("Controller found and supports haptics.");
            targetDevice.SendHapticImpulse(0, 1f, 0.1f); // test
        }
        else
        {
            Debug.LogWarning("Controller does not support haptics.");
        }
    }


    public async UniTask RumbleBurst(float intensity, float duration)
    {
        if (!targetDevice.isValid || !targetDevice.TryGetHapticCapabilities(out var capabilities) || !capabilities.supportsImpulse)
            return;

        targetDevice.SendHapticImpulse(0, intensity, duration);
        await UniTask.Delay((int)(duration * 1000));
    }

    public async UniTaskVoid StartEngineRumble(float minIntensity, float maxIntensity, float rampTime)
    {

        if (!targetDevice.isValid)
            return;

        if (isEngineRumbling)
            return;
        Debug.Log("RUMBLE ENGINE");
        StopRumble();
        rumbleTokenSource = new CancellationTokenSource();
        var token = rumbleTokenSource.Token;

        isEngineRumbling = true;

        float elapsed = 0f;
        float step = 0.1f;

        // Ramp up phase
        while (elapsed < rampTime && !token.IsCancellationRequested)
        {
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, elapsed / rampTime);
            targetDevice.SendHapticImpulse(0, intensity, step);
            elapsed += step;
            await UniTask.Delay((int)(step * 1000), cancellationToken: token);
        }

        // Sustain phase
        while (!token.IsCancellationRequested)
        {
            targetDevice.SendHapticImpulse(0, maxIntensity, step);
            await UniTask.Delay((int)(step * 1000), cancellationToken: token);
        }

        isEngineRumbling = false;
    }


    public void StopRumble()
    {
        if (rumbleTokenSource != null && !rumbleTokenSource.IsCancellationRequested)
        {
            rumbleTokenSource.Cancel();
        }
        isEngineRumbling = false;
    }


}
