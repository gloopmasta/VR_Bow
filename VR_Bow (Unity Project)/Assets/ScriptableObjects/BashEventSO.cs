using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Events/Bash Events")]
public class BashEventSO : MonoBehaviour
{
    public event Action OnLaunchingBash;

    public void RaiseLaunchingBash() => OnLaunchingBash?.Invoke();
}
