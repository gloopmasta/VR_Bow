using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/BowEvents")]
public class BowEventsSO : ScriptableObject
{
    public Action<float> OnChargeLevelChanged;  // charge 0–1
    public Action OnArrowReleased;              // flash white
    public Action OnBowIdle;                    // back to black
}