using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/JumpPad")]
public class JumpPadEventSO : MonoBehaviour
{
    public event Action OnEnterJumpPad;

    public void RaiseEnterJumpPad() => OnEnterJumpPad?.Invoke();
}
