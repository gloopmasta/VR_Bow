using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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

    private void HandleSlowTimeEnter(float factor)
    {
        // apply timeSlow to all ITimeScalables
        foreach (var sc in scalables) sc.OnTimeScaleChanged(factor);
    }
    private void HandleSlowTimeExit()
    {
        // revert all ItimeScalables back to 1f speed
        foreach (var sc in scalables) sc.OnTimeScaleChanged(1f);
    }


}
