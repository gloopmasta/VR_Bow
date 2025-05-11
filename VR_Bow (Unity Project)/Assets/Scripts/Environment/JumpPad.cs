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
        if (other.CompareTag("Player"))
        {
            DriveControls playerDriveScript = other.GetComponent<DriveControls>();
            playerDriveScript.Launch(30f);


            slowtime.RaiseSlowTimeEnter(0.2f, 3f); //raise slow down time for 3 seconds
            switchEvents.RaiseEnterDSSwitchTime(); //raise entering switch time for the player
        }

    }

}
