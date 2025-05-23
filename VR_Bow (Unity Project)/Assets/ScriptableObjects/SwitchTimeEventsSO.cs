using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/SwitchTime Events")]
public class SwitchTimeEventsSO : ScriptableObject
{
    public event Action OnEnterDSSwitchTime;
    public event Action OnExitDSSwitchTime;

    public void RaiseEnterDSSwitchTime() => OnEnterDSSwitchTime?.Invoke();
    public void RaiseExitDSSwitchTime() => OnExitDSSwitchTime?.Invoke();
}

