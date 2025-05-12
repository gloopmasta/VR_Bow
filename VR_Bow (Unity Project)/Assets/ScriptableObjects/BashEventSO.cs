using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Events/Bash Events")]
public class BashEventSO : ScriptableObject
{
    public event Action OnLaunchingBash;

    public void RaiseLaunchingBash() => OnLaunchingBash?.Invoke();
}
