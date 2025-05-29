using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    private static BeatManager _instance;
    private static readonly object _threadLock = new object();

    public static BeatManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_threadLock)
                {
                    _instance = FindObjectOfType<BeatManager>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(BeatManager).Name);
                        _instance = singletonObject.AddComponent<BeatManager>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
            }
            return _instance;
        }
    }

    public delegate void BeatManagerDelegate();
    public static event BeatManagerDelegate OnBeatChange;

    [Header("Audio Settings")]
    public float bpm = 0f;
    public float beatOffset;
    public AudioSource audioSource;
    public float sampledTime;
    public float beatCount;
    public int intBeatCount;
    public float heartBeat;

    private float lastReportedPitch = 1f;
    private float pitchAdjustedTime = 0f;

    private void Start()
    {
        if (bpm == 0f)
            bpm = UniBpmAnalyzer.AnalyzeBpm(audioSource.clip);

        intBeatCount = 0;
        lastReportedPitch = audioSource.pitch;
    }

    private void Update()
    {
        // Track pitch-adjusted time
        if (audioSource.pitch > 0.01f) // Avoid division by zero
        {
            pitchAdjustedTime += Time.deltaTime * lastReportedPitch / audioSource.pitch;
            lastReportedPitch = audioSource.pitch;
        }

        // Calculate beat count using pitch-adjusted time
        beatCount = beatOffset + (pitchAdjustedTime / GetBeatLength());
        heartBeat = beatCount - (int)beatCount;
        intBeatCount = (int)beatCount;

        CheckForNewBeat();
    }

    private int lastInterval;

    void CheckForNewBeat()
    {
        if (Mathf.FloorToInt(beatCount) != lastInterval)
        {
            lastInterval = Mathf.FloorToInt(beatCount);
            OnBeatChange?.Invoke();
        }
    }

    float GetBeatLength()
    {
        return 60f / bpm;
    }

    // Call this when you start playing after a pitch change to reset timing
    public void ResetTiming()
    {
        pitchAdjustedTime = 0f;
        lastReportedPitch = audioSource.pitch;
    }
}