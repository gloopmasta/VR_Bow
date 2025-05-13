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
}
