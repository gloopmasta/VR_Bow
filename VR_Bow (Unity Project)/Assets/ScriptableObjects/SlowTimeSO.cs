using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;

[CreateAssetMenu(menuName = "Other ScriptableObjects/SlowTime")]
public class SlowTimeSO : ScriptableObject
{
    //Events
    public event Action<float> OnSlowTimeEnter;
    public event Action OnSlowTimeExit;

    public void RaiseSlowTimeEnter(float factor) => OnSlowTimeEnter?.Invoke(factor);
    public void RaiseSlowTimeExit() => OnSlowTimeExit?.Invoke();

   

}
