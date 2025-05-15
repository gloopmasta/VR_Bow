using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Libraries/Environment Library")]
public class EnvironmentLibrary : ScriptableObject
{
    [Header("Powerups")]
    public GameObject arrowUp;
    public GameObject slowtimeUp;
    public GameObject trail;
    public GameObject spinningLaser;

    [Header("Tracks")]
    public List<GameObject> trackList;


}
