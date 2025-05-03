using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    public int maxHp = 3;
    public int maxArrowCount = 6;
    public float maxFuel = 100f;

    
}
