using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Other ScriptableObjects/SlowTime")]
public class SlowTimeSO : ScriptableObject
{
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
        Time.timeScale = slowAmount;
        await UniTask.WaitForSeconds(duration, true);
        Time.timeScale = 1f;
        return true;
    }
}
