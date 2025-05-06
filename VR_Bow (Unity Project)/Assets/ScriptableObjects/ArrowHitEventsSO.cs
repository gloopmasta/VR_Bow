using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Player/ArrowHitEvents")]
public class ArrowHitEventsSO : ScriptableObject
{
    public event Action<int> OnArrowHitEnemy;

    public void RaiseScoreHitEnemy(int amount)
    {
        OnArrowHitEnemy?.Invoke(amount);
    }
}
