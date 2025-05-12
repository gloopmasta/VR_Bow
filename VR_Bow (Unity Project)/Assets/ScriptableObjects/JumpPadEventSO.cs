using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/JumpPad")]
public class JumpPadEventSO : ScriptableObject
{
    public event Action OnEnterJumpPad;

    public void RaiseEnterJumpPad() => OnEnterJumpPad?.Invoke();
}
