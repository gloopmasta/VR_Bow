using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [Header("Time Scaling")]
    public SlowTimeSO slowTimeEvent;


    public GameObject player;
    public List<GameObject> enemies = new List<GameObject>();
    private List<ITimeScalable> scalables = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        slowTimeEvent.OnSlowTimeEnter += HandleSlowTimeEnter;
        slowTimeEvent.OnSlowTimeExit += HandleSlowTimeExit;
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
        => scalables.Add(obj);

    public void Unregister(ITimeScalable obj)
        => scalables.Remove(obj);

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
        // apply timeSlow to all ITimeScalables
        foreach (var sc in scalables) sc.OnTimeScaleChanged(factor);

        //Slow down music pitch
        await SlowDownMusic();
        Debug.Log("Slowed game time to: " + factor);
    }
    private async void HandleSlowTimeExit()
    {
        // revert all ItimeScalables back to 1f speed
        foreach (var sc in scalables) sc.OnTimeScaleChanged(1f);
        await SpeedUpMusic();
        Debug.Log("resumed game time to normal");
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
        float startPitch = BeatManager.Instance.audioSource.pitch;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            BeatManager.Instance.audioSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);
            await Task.Yield();
        }

        BeatManager.Instance.audioSource.pitch = targetPitch;
    }

}
