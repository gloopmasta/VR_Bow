using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    public readonly int maxHp = 3;
    public readonly int maxArrowCount = 6;
    public readonly float maxFuel = 100f;

    //TODO : read only properties
}
