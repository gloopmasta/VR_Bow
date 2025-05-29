using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    private static BeatManager _instance;

    // Lock for thread safety (not strictly necessary for Unity, but shown for completeness)
    private static readonly object _threadLock = new object();

    public static BeatManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_threadLock)
                {
                    // Try to find an existing instance of the BeatManager
                    _instance = FindObjectOfType<BeatManager>();

                    // If no instance is found, create a new GameObject with the BeatManager component
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(BeatManager).Name);
                        _instance = singletonObject.AddComponent<BeatManager>();

                        // Optional: Make the BeatManager persist between scenes
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
    public float bpm = 0f; //default value
    public float beatOffset;
    public AudioSource audioSource;
    //[SerializeField] private Intervals[] intervals;
    public float sampledTime;
    public float beatCount;
    public int intBeatCount;
    public float heartBeat;

    private void Start()
    {
        if (bpm == 0f)
            bpm = UniBpmAnalyzer.AnalyzeBpm(audioSource.clip);

        intBeatCount = 0;
    }

    private void Update()
    {

        //foreach (var interval in intervals) 
        //{
        //    sampledTime = beatOffset + (audioSource.timeSamples / (audioSource.clip.frequency * interval.GetBeatLength(bpm)) ); //gets time elapsed in beats
        //    interval.CheckForNewBeat(sampledTime);
        //}


        beatCount = beatOffset + (audioSource.timeSamples / (audioSource.clip.frequency * GetBeatLength())); //gets beats in float

        heartBeat = beatCount - (int)beatCount; //value that resets every time 0-1

        intBeatCount = (int)beatCount;

        CheckForNewBeat();
    }

    private int lastInterval;

    void CheckForNewBeat()
    {
        if (Mathf.FloorToInt(beatCount) != lastInterval)
        {
            lastInterval = Mathf.FloorToInt(beatCount);

            if (OnBeatChange != null)
            {
                OnBeatChange();
            }
            //Debug.Log("NOTIFY");
        }
    }

    float GetBeatLength()
    {
        return 60f / bpm;
    }

}

//[System.Serializable]
//public class Intervals
//{
//    [SerializeField] public float steps;
//    [SerializeField] private UnityEvent trigger;
//    private int lastInterval;

//    public float GetBeatLength(float bpm)
//    {
//        return 60f / (bpm * steps); //beats per second STEPS: for half beats
//    }

//    public void CheckForNewBeat (float interval)
//    {
//        if (Mathf.FloorToInt(interval) != lastInterval)//if current interval i sbigger than last interval
//        {
//            lastInterval = Mathf.FloorToInt(interval); //lastinterval is now interval
//            trigger.Invoke();
//        }
//    }
//}

public enum Rating
{
    Undetermined,
    TooEarly,
    Early,
    Late,
    TooLate
}
