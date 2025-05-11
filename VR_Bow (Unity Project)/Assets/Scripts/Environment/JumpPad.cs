using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float lauchStrength = 400f;
    public SwitchTimeEventsSO switchEvents;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DriveControls playerDriveScript = other.GetComponent<DriveControls>();
<<<<<<< Updated upstream
            if (!playerDriveScript.enabled)
            {
                //enable the script, launch the player than disable again
                playerDriveScript.enabled = true;
                playerDriveScript.Launch(lauchStrength);
                playerDriveScript.enabled = false;
            }
            else
            {
                playerDriveScript.Launch(30f);
            }
=======
            playerDriveScript.Launch(lauchStrength);
>>>>>>> Stashed changes

            switchEvents.RaiseEnterDSSwitchTime();
        }
    }
}
