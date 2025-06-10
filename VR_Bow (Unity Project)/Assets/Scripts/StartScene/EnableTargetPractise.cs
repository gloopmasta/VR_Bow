using UnityEngine;
using TMPro;
using System.Collections;
using System.Diagnostics;

public class EnableTargetPractise : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private GameObject targets;
    [SerializeField] private GameObject copy;

    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 60f; // 1 minute (customizable)
    [SerializeField] private TMP_Text timerText;   // Assign in Inspector
    [SerializeField] private GameObject timerUI;   // Assign in Inspector

    private Stopwatch timer = new Stopwatch();
    private bool isChallengeActive = false;
    private Coroutine timerCoroutine;

    private void OnEnable()
    {
        copy = Instantiate(targets);
        targets.SetActive(false);
        copy.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow") && !isChallengeActive)
        {
            ActivateTargets();
            StartTimer();
        }
    }

    void ActivateTargets()
    {
        if (copy != null)
            Destroy(copy);

        copy = Instantiate(targets);
        copy.SetActive(true);
        isChallengeActive = true;
    }

    void StartTimer()
    {
        timerUI.SetActive(true);
        timer.Reset();
        timer.Start();

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(UpdateTimerUI());
    }

    void StopTimer(bool success)
    {
        timerUI.SetActive(false);
        timer.Stop();
        isChallengeActive = false;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        if (success)
            timerText.text = $"Completed in: {timer.Elapsed.TotalSeconds:F2}s!";
        else
            timerText.text = "Time's up!";
    }

    IEnumerator UpdateTimerUI()
    {
        float startTime = (float)timer.Elapsed.TotalSeconds;

        while (isChallengeActive)
        {
            float elapsed = (float)timer.Elapsed.TotalSeconds;
            float remaining = Mathf.Max(0, timeLimit - elapsed);

            // Update UI (e.g., "00:59")
            timerText.text = $"{Mathf.FloorToInt(remaining / 60):00}:{Mathf.FloorToInt(remaining % 60):00}";

            // Timeout after 1 minute
            if (elapsed >= timeLimit)
            {
                StopTimer(false);
                break;
            }

            // Check if all targets are destroyed
            if (AreAllTargetsDestroyed())
            {
                StopTimer(true);
                break;
            }

            yield return null;
        }
    }

    bool AreAllTargetsDestroyed()
    {
        if (copy == null) return true;

        foreach (Transform child in copy.transform)
        {
            if (child.gameObject.activeInHierarchy)
                return false;
        }
        return true;
    }
}