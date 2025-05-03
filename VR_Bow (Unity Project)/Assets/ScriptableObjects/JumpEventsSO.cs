using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Jump Events")]
public class JumpEventsSO : ScriptableObject
{
    public event Action OnJump;
    public event Action OnLand;

    public void RaiseJump() => OnJump?.Invoke();
    public void RaiseLand() => OnLand?.Invoke();
}
