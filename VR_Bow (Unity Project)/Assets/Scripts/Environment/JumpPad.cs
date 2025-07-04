using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float lauchStrength = 8f;
    public SwitchTimeEventsSO switchEvents;
    public SlowTimeSO slowtime;
    public JumpPadEventSO jpEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Activate(other.gameObject);
        }
    }

    public void Activate(GameObject player)
    {
        DriveControls playerDriveScript = player.GetComponent<DriveControls>();

        playerDriveScript.Launch(lauchStrength, 1f);
        //switchEvents.RaiseEnterDSSwitchTime();
        //Debug.Log("Jumppad called enter DS switch time -> should be active now");
        jpEvent.RaiseEnterJumpPad();
    }

}
