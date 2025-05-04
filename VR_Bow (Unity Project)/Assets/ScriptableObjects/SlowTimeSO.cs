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
    public event Action OnSlowTimeEnter;
    public event Action OnSlowTimeExit;

    public void RaiseSlowTimeEnter() => OnSlowTimeEnter?.Invoke();
    public void RaiseSlowTimeExit() => OnSlowTimeExit?.Invoke();

    public float slowAmount = 0.2f;

    public void EnterSlowTime()
    {
        Time.timeScale = slowAmount;
    }
    public void ExitSlowTime()
    {
        Time.timeScale = 1f;
    }

    public async Task<bool> SlowTime(float duration)
    {
        RaiseSlowTimeEnter();
        Time.timeScale = slowAmount;

        await UniTask.WaitForSeconds(duration, true) ;

        RaiseSlowTimeExit();
        Time.timeScale = 1f;
        return true;
    }
}
