using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    void Update()
    {
        if (PlayerManager.Instance.State == PlayerState.Driving)
        {
            HandleDrivingState();
        }
        else if (PlayerManager.Instance.State == PlayerState.Shooting)
        {
            HandleShootingState();
        }
    }

    void HandleDrivingState()
    {
        
    }

    void HandleShootingState()
    {
        
    }

    void SwitchState(PlayerState newState)
    {
        PlayerManager.Instance.State = newState;
    }

    // Sample condition methods:
    //public bool IsAimingBow() { return BowInputDetector.IsAiming(); }
    public bool IsGrounded() 
    {
        GroundCheck gc = GetComponentInChildren<GroundCheck>();
        if (gc == null)
        {
            Debug.LogError("no GrounCheck script found in children for StateController");
            return false;
        }

        return gc.IsGrounded(); 
    }
}
