using UnityEngine;
using TMPro;
using System.Collections;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnableTargetPractise : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private GameObject targets;
    [SerializeField] private GameObject copy;

    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float resultsDisplayTime = 2f;
    //[SerializeField] private GameObject screenTimer;
    [SerializeField] private TMP_Text worldTimer;
    [SerializeField] private GameObject startScreenUI;

    private Stopwatch timer = new Stopwatch();
    private bool isChallengeActive;
    private CancellationTokenSource cts;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow") && !isChallengeActive)
        {
            StartChallenge().Forget();
        }
    }

    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 40, 300, 40), "trigger targets"))
    //    {
    //        StartChallenge().Forget();
    //    }
    //}

    private async UniTaskVoid StartChallenge()
    {
        // Cleanup any existing challenge
        await CleanupExistingChallenge();

        startScreenUI.GetComponent<Animator>().CrossFade("StartScreenFadeOut", 0f);
        await UniTask.WaitForSeconds(1);
        startScreenUI.SetActive(false);

        //screenTimer.SetActive(true);

        // Setup new challenge
        copy = Instantiate(targets);
        copy.SetActive(true);
        isChallengeActive = true;
        cts = new CancellationTokenSource();

        // Run concurrent tasks
        await UniTask.WhenAll(
            RunTimer(cts.Token),
            MonitorTargets(cts.Token)
        );
    }

    private async UniTask CleanupExistingChallenge()
    {
        if (copy != null)
        {
            Destroy(copy);
            await UniTask.Yield(); // Allow one frame for destruction
        }

        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private async UniTask RunTimer(CancellationToken token)
    {
        timer.Restart();

        try
        {
            while (!token.IsCancellationRequested)
            {
                float remaining = Mathf.Max(0, timeLimit - (float)timer.Elapsed.TotalSeconds);
                timerText.text = $"{(float)timer.Elapsed.TotalSeconds:F2}"; // Shows "12.345" seconds

                if (remaining <= 0)
                {
                    //screenTimer.SetActive(false);
                    HandleTimeOut();
                    break;
                }

                await UniTask.Yield();
            }
        }
        finally
        {
            timer.Stop();
        }
    }

    private void HandleTimeOut()
    {
        worldTimer.text = "00:00"; // Timeout display
        startScreenUI.SetActive(true);
        isChallengeActive = false;
        copy.SetActive(false);
        cts?.Cancel();
    }

    private async UniTask MonitorTargets(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield();

            if (AreAllTargetsDestroyed())
            {
                float completionTime = (float)timer.Elapsed.TotalSeconds;
                timerText.text = $"Clear! Time: {completionTime:F2}s";

                await UniTask.Delay((int)(resultsDisplayTime * 1000), cancellationToken: token);

                worldTimer.text = $"{completionTime:F2}";
                startScreenUI.SetActive(true);
                
                

                isChallengeActive = false;
                cts?.Cancel();
                break;
            }
        }
    }

    private bool AreAllTargetsDestroyed()
    {
        if (copy == null) return true;

        foreach (Transform child in copy.transform)
        {
            if (child.gameObject.activeInHierarchy)
                return false;
        }
        return true;
    }

    private void OnDisable()
    {
        // Only cancel if not already disposed
        if (cts != null && !cts.IsCancellationRequested)
        {
            try
            {
                cts?.Cancel();
            }
            finally
            {
                cts?.Dispose();
                cts = null; // Clear the reference
            }
        }
    }
}