using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;

public class TutorialActivator : MonoBehaviour
{
    private bool alreadyActivated = false;
    [SerializeField] private bool triggerSteeringMessage = false;
    [SerializeField] private bool triggerJumpingMessage = false;
    [SerializeField] private bool triggerTurningMessage = false;
    [SerializeField] private bool triggerShootingMessage = false;

    private void OnTriggerEnter(Collider other)
    {
        Tutorial tut = other.GetComponent<Tutorial>();
        if (tut == null || alreadyActivated) //if doesn't have tutorioal or already activated -> return
            return;

        if (triggerSteeringMessage)
            tut.steeringActivated = true;

        if (triggerJumpingMessage)
            tut.jumpingActivated = true;

        if(triggerShootingMessage)
            tut.shootingActivated = true;

    }
}
