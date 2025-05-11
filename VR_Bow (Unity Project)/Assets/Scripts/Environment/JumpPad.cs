using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float lauchStrength = 400f;
    public SwitchTimeEventsSO switchEvents;
    public SlowTimeSO slowtime;

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    DriveControls playerDriveScript = other.GetComponent<DriveControls>();
        //    if (!playerDriveScript.enabled)
        //    {
        //        //enable the script, launch the player than disable again
        //        playerDriveScript.enabled = true;
        //        playerDriveScript.Launch(lauchStrength);
        //        playerDriveScript.enabled = false;
        //    }
        //    else
        //    {
        //        playerDriveScript.Launch(30f);
        //    }

        //    switchEvents.RaiseEnterDSSwitchTime();
        //}

        slowDownTheDamnTime().Forget();
    }

    async UniTaskVoid slowDownTheDamnTime()
    {
        Time.timeScale = 0.2f;
        await UniTask.Delay(4000);
        Time.timeScale = 1f;
    }
}
