using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading;
using PandaBT;

public class StateController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private Player player;
    [SerializeField] private BowControls bowControls;
    [SerializeField] private DriveControls driveControls;

    private void Start()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
            Debug.Assert(player != null, "no Player script attached or injected");
        }

        SwitchState(PlayerState.Driving);
    }

    void Update() // TODO: CHANGE TO EVENT BASED
    {
        if (player.State == PlayerState.Driving)
        {
            HandleDrivingState();
        }
        else if (player.State == PlayerState.Shooting)
        {
            HandleShootingState();
        }
    }

    void HandleDrivingState()
    {
        //on jump, switch to bow
        if (!IsGrounded())
        {
            SwitchState(PlayerState.Shooting);
        }
    }

    void HandleShootingState()
    {
        //on land -> switch to drive
        if (!IsGrounded())
        {
            SwitchState(PlayerState.Driving);
        }
    }

    void SwitchState(PlayerState newState)
    {
        player.State = newState;

        switch (newState) 
        { 
        case PlayerState.Driving:
                driveControls.enabled = true;
                bowControls.enabled = false;
                break; 

        case PlayerState.Shooting:
                driveControls.enabled = false;
                bowControls.enabled = true;
                break;
        }
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
