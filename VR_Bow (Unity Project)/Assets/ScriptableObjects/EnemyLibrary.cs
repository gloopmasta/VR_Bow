using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Libraries/Enemy Library")]
public class EnemyLibrary : ScriptableObject
{
    [Header("Enemy Prefabs")]
    public List<GameObject> easyEnemies;
    public List<GameObject> mediumEnemies;
    public List<GameObject> hardEnemies;
    public List<GameObject> bosses;

    [Space(20)]
    [Header("Enemy Projectiles & Utilities")]
    public GameObject trailSegment;
    public GameObject rocket;
    public GameObject homingRocket;
    public GameObject laser;
    public GameObject homingLaser;

    public GameObject[] GetRandomMixedPool(float easyRatio, float mediumRatio, float hardRatio, int totalCount = 3)
    {
        List<GameObject> result = new List<GameObject>();

        int easyCount = Mathf.RoundToInt(totalCount * easyRatio);
        int mediumCount = Mathf.RoundToInt(totalCount * mediumRatio);
        int hardCount = Mathf.RoundToInt(totalCount * hardRatio);

        AddRandomFromList(easyEnemies, easyCount, result);
        AddRandomFromList(mediumEnemies, mediumCount, result);
        AddRandomFromList(hardEnemies, hardCount, result);

        return result.ToArray();
    }

    private void AddRandomFromList(List<GameObject> source, int count, List<GameObject> target)
    {
        if (source == null || source.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = source[Random.Range(0, source.Count)];
            target.Add(prefab);
        }
    }

}
