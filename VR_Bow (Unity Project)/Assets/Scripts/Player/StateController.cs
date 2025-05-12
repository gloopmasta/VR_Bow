using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PandaBT;
using PandaBT.Runtime;
using System.Threading.Tasks;
using System;
using System.Threading;

public class StateController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerDataSO playerData;
    [SerializeField] private BowControls bowControls;
    [SerializeField] private DriveControls driveControls;
    [SerializeField] private BowModelHandler bowModelHandler;
    [SerializeField] private JumpEventsSO jumpEvents;
    [SerializeField] private SwitchTimeEventsSO switchEvents;
    [SerializeField] public SlowTimeSO slowTime;
    [SerializeField] public JumpPadEventSO jumpPadEvent;
    [SerializeField] public BashEventSO bashEvent;

    [Header("Bow Rotation")]
    [SerializeField] private Transform bowTransform;
    [SerializeField] private float vertAngleTarget = 90f;
    [SerializeField] private float horAngleTarget = 0f;
    [SerializeField] private float tolerance = 20f;

    [Header("Aditional Settings")]
    [SerializeField][PandaVariable] public bool isGrounded;
    [SerializeField] private bool canEnterSwitchtime = false;
    [SerializeField] private bool switchTimeActive = false;
    [SerializeField] private bool usedJumpPad = false;
    [SerializeField] private bool didLaunchingBash = false;
    [SerializeField] private bool slowTimeActive = false;


    private void OnEnable()
    {
        jumpEvents.OnJump += () => isGrounded = false;
        jumpEvents.OnLand += () => 
        { 
            isGrounded = true;  usedJumpPad = false;  
        };
        switchEvents.OnEnterDSSwitchTime += () => canEnterSwitchtime = true;

        jumpPadEvent.OnEnterJumpPad += () => usedJumpPad = true;
        bashEvent.OnLaunchingBash += () => didLaunchingBash = true;

        //slowTime.OnSlowTimeExit += () => switchTimeActive = false;
        //slowTime.OnSlowTimeEnter += () => switchTimeActive = true;
    }

    
    private void Start()
    {
        if (player == null)
        {
            player = GetComponent<Player>();
            Debug.Assert(player != null, "no Player script attached or injected");
        }

        SetState(PlayerState.Driving);
    }

    void Update() // TODO: CHANGE TO EVENT BASED
    {

    }

    [PandaTask]
    void SetState(PlayerState newState)
    {
        player.State = newState;

        // Manage Scripts
        switch (newState)
        {
            case PlayerState.Driving:
                driveControls.canDrive = true;
                bowControls.enabled = false;
                bowModelHandler.isLocked = true;
                break;

            case PlayerState.Shooting:
                driveControls.canDrive = false;
                bowControls.enabled = true;
                bowModelHandler.isLocked = false;
                break;
        }

        PandaTask.Succeed();
    }

    [PandaTask] public bool IsShootingMode() { return player.State == PlayerState.Shooting; }
    [PandaTask] public bool IsDrivingMode() { return player.State == PlayerState.Driving; }
    [PandaTask] public bool ISwitchTime() { return switchTimeActive; }

    [PandaTask]
    public async Task<bool> SwitchTime()
    {
        try
        {
            switchTimeActive = true;
            slowTime.RaiseSlowTimeEnter(playerData.SlowAmount); //slow time down

            // Start both tasks
            var bowTask = WaitUntilBowVertical();   //Wait until the bow vertical
            var delayTask = UniTask.WaitForSeconds(playerData.Switchtime).AsTask(); //wait 3 seconds

            // Wait for whichever completes first
            var completedFirst = await Task.WhenAny(bowTask, delayTask);

            // If bow task completed first and returned true
            if (completedFirst == bowTask && await bowTask)
            {
                return true;
            }

            // Otherwise (timeout or false result)
            return false;
        }
        finally
        {
            slowTime.RaiseSlowTimeExit();
            switchTimeActive = false;
        }
    }

    [PandaTask] 
    public async Task<bool> SlowTime()
    {

        if (usedJumpPad) //slowtime from jumppad
        {
            await JumpPadSlowtime();
        }
        else if (didLaunchingBash) //SlowTime from launchingBash
        {
            await LaunchingBashSlowtime();
        }

        return true;
    }

    private async Task<bool> JumpPadSlowtime()
    {
        var cts = new CancellationTokenSource();

        try
        {
            float totalSlowTime = playerData.SlowtimeFromJumppad + player.SlowTime; //get free slowtime from jumppad + playerSlowtime

            slowTime.RaiseSlowTimeEnter(playerData.SlowAmount); //START SLOWTIME

            // Start both tasks
            //landTask will not immediately be aborted, it will just wait until isGrounded is true and return true either way
            var landTask = WaitUntilGrounded();                                                             //Wait until grounded
            var delayTask = UniTask.WaitForSeconds(totalSlowTime, cancellationToken: cts.Token).AsTask();   //Wait until all slowtime Passes

            // Wait for either task to complete
            var completedFirst = await Task.WhenAny(landTask, delayTask);

            // Cancel whichever task is still running
            cts.Cancel();

            return true;
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, no need to handle specially
            return true;
        }
        finally
        {
            // Ensure cleanup happens in all cases
            slowTime.RaiseSlowTimeExit();               //Stop SlowTime when player lands or when slowtime is over
            player.SlowTime = 0f;
            cts.Dispose();
        }
    }
    private async Task<bool> LaunchingBashSlowtime()
    {
        //NO player slowtime

        slowTime.RaiseSlowTimeEnter(playerData.SlowAmount);

        await UniTask.WaitForSeconds(playerData.SlowtimeFromBash); //get free slowTime from JumpPad

        slowTime.RaiseSlowTimeExit();

        didLaunchingBash = false; //turn off didLaunchingBash

        return true;
    }
    


    // Checks rotation around the local X-axis
    private float GetBowTiltAngle()
    {
        // Assuming bow tilts up on local X axis
        float angle = bowTransform.localEulerAngles.z;
        // Normalize angle to 0–180 for comparison
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    [PandaTask]
    public void MoveForwardSlowly()
    {
        //move forward slowly
        //if (PandaTask.isStarting)
        //transform.Translate(Vector3.forward * 5f * Time.deltaTime); //run indefinitely

    }

    


    [PandaTask]
    public void DisplaySwitchMessage()
    {
        // Display a message that says you need to tilt your back to drive mode
    }

    [PandaTask]
    public async Task<bool> WaitUntilGrounded()
    {
        await UniTask.WaitUntil(() => isGrounded);
        return true;
    }
    [PandaTask]
    public async Task<bool> WaitUntilJump()
    {
        await UniTask.WaitUntil(() => !isGrounded);
        return true;
    }

    [PandaTask]
    public async Task<bool> WaitUntilSwitchtime()
    {
        await UniTask.WaitUntil(() => canEnterSwitchtime);
        canEnterSwitchtime = false;
        return true;
    }

    [PandaTask]
    public async Task<bool> WaitUntilBowVertical()
    {
        await UniTask.WaitUntil(() =>
            Mathf.Abs(GetBowTiltAngle() - vertAngleTarget) <= tolerance || !switchTimeActive);

        //if (!switchTimeActive)
        //{
        //    return false;
        //}

        return true;
    }

    [PandaTask]
    public async Task<bool> WaitUntilBowHorizontal()
    {
        await UniTask.WaitUntil(() =>
            Mathf.Abs(GetBowTiltAngle() - horAngleTarget) <= tolerance);

        return true;
    }

    // Sample condition methods:
    //public bool IsAimingBow() { return BowInputDetector.IsAiming(); }
    //public bool IsGrounded() 
    //{
    //    GroundCheck gc = GetComponentInChildren<GroundCheck>();
    //    if (gc == null)
    //    {
    //        Debug.LogError("no GrounCheck script found in children for StateController");
    //        return false;
    //    }

    //    return gc.IsGrounded(); 
    //}
}
