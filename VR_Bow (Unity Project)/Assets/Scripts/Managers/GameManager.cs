using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Time Scaling")]
    public SlowTimeSO slowTimeEvent;

    [Header("Debug TimeScale Toggle")]
    [SerializeField] private bool enableTimeScaleTesting = true;
    [SerializeField] private KeyCode toggleTimeKey = KeyCode.T;
    [SerializeField] private float testSlowTimeFactor = 0.3f;

    public float CurrentTimeScale => isInSlowTime ? testSlowTimeFactor : 1f;
    public GameObject player;
    public List<GameObject> enemies = new List<GameObject>();
    private List<ITimeScalable> scalables = new();

    private bool isInSlowTime = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        slowTimeEvent.OnSlowTimeEnter += HandleSlowTimeEnter;
        slowTimeEvent.OnSlowTimeExit += HandleSlowTimeExit;
    }

    private void Update()
    {
        Debug.Log("GameManager Update running");

        if (!enableTimeScaleTesting) return;

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("Pressed L - Slowmo");
            isInSlowTime = true;
            slowTimeEvent.RaiseSlowTimeEnter(testSlowTimeFactor);
        }
        else if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            Debug.Log("Pressed K - Back to normal");
            isInSlowTime = false;
            slowTimeEvent.RaiseSlowTimeExit();
        }
    }

    public bool IsInSlowTime()
    {
        return isInSlowTime;
    }

    


    public void StartLevelOne()
    {
        // Initialize level logic
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
    }

    public void Register(ITimeScalable obj)
    {
        if (!scalables.Contains(obj))
            scalables.Add(obj);
    }

    public void Unregister(ITimeScalable obj)
    {
        if (scalables.Contains(obj))
            scalables.Remove(obj);
    }

    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    public void ClearAllEnemies()
    {
        enemies.Clear();
        player = null;
    }

    private async void HandleSlowTimeEnter(float factor)
    {
        foreach (var sc in scalables)
            sc.OnTimeScaleChanged(factor);

        await SlowDownMusic();
        Debug.Log("Slowed game time to: " + factor);
    }

    private async void HandleSlowTimeExit()
    {
        foreach (var sc in scalables)
            sc.OnTimeScaleChanged(1f);

        await SpeedUpMusic();
        Debug.Log("Resumed game time to normal");
    }

    public async Task SlowDownMusic(float duration = 0.2f)
    {
        await ChangePitch(0.7f, duration);
    }

    public async Task SpeedUpMusic(float duration = 0.2f)
    {
        await ChangePitch(1f, duration);
    }

    private async Task ChangePitch(float targetPitch, float duration)
    {
        AudioSource audioSource = BeatManager.Instance.audioSource;
        float startPitch = audioSource.pitch;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            audioSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);
            await Task.Yield();
        }

        audioSource.pitch = targetPitch;
    }
}
