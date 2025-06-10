using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PandaBT;
using PandaBT.Runtime;
using System.Threading.Tasks;
using System;
using System.Threading;
using Cysharp.Threading.Tasks.CompilerServices;

public class StateController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerDataSO playerData;
    [SerializeField] private LevelEventsSO levelEvents;
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private ShootingMode bowControls;
    [SerializeField] private DriveControls driveControls;
    [SerializeField] private BowModelHandler bowModelHandler;
    [SerializeField] private JumpEventsSO jumpEvents;
    [SerializeField] private SwitchTimeEventsSO switchEvents;
    [SerializeField] public SlowTimeSO slowTime;
    [SerializeField] public JumpPadEventSO jumpPadEvent;
    [SerializeField] public BashEventSO bashEvent;
    [SerializeField] public RumbleManager rumble;
    [SerializeField] private OffRoadTracker offRoad;

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
        if (this == null) return;

        jumpEvents.OnJump += () =>
        {
            isGrounded = false;
            //bowControls.canShoot = true; //reenable being able to shoot
            //offRoad.enabled = false; //disable offroadtracker
            //rumble.StopRumble();
        };

        jumpEvents.OnLand += () =>
        {
            isGrounded = true;
            usedJumpPad = false;
            //offRoad.enabled = true; //enable offroadtracker
            //rumble.StartEngineRumble(0f, 1f, 2f).Forget();
        };
        switchEvents.OnEnterDSSwitchTime += () =>
        {
            canEnterSwitchtime = true;
            bowControls.canShoot = true; //reenable being able to shoot
        };

        jumpPadEvent.OnEnterJumpPad += () =>
        {
            Debug.Log("enterJumpPad triggered from statemanager");
            usedJumpPad = true;
            isGrounded = false;
            SetState(PlayerState.Shooting);
            driveControls.enabled = false;
            offRoad.enabled = false;
            bowControls.canShoot = true; //reenable being able to shoot
            //rumble.StopRumble();
            JumpPadSlowtime().Forget();
        };

        bashEvent.OnLaunchingBash += () =>
        {
            didLaunchingBash = true;
            bowControls.canShoot = true; //reenable being able to shoot
        };

        levelEvents.OnLevelOneStart += () =>
        {
            //rumble.StartEngineRumble(0.1f, 1f, 3f).Forget();
            AfterStartPressed().Forget();
        };

        levelEvents.OnLevelOneRestart += () => offRoad.enabled = false;

        levelEvents.OnLevelOneLose += () => OnGameEnd();
        levelEvents.OnLevelOneWin += () => OnGameEnd();
    }

    
    private void Start()
    {
        if (gameSettings.useBowController) //if using bow
        {
            bowControls = GetComponent<BowShooting>(); //set bowControls script ot bbow
            GetComponent<ControllerShooting>().enabled = false; //disable controller
        }
        else //if using controllers
        {
            bowControls = GetComponent<ControllerShooting>();
            GetComponent<BowShooting>().enabled = false;
        }

        if (player == null)
        {
            player = GetComponent<Player>();
            Debug.Assert(player != null, "no Player script attached or injected");
        }

        SetState(PlayerState.Driving);
    }

    

    [PandaTask]
    public bool SetState(PlayerState newState)
    {
        if (player.State == newState)
        {
            return true;
        }
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

        return true;
    }

    [PandaTask] public bool IsShootingMode() { return player.State == PlayerState.Shooting; }
    [PandaTask] public bool IsDrivingMode() { return player.State == PlayerState.Driving; }
    [PandaTask] public bool IsSwitchTime() { return switchTimeActive; }
    [PandaTask] public bool ActivateSwitchTime() { switchTimeActive = true; return true; }
    [PandaTask] public bool DisableSwitchTime() { switchTimeActive = false; return true; }
    [PandaTask] public bool IsGrounded() { return isGrounded; }
    [PandaTask] public bool IsInAir() { return !isGrounded; }

    //[PandaTask]
    //public async Task<bool> SwitchTime()
    //{
    //    CancellationTokenSource cts = new CancellationTokenSource();
    //    try
    //    {
    //        GetComponent<PlayerUIManager>().rotateBowPanel.SetActive(true);
    //        switchTimeActive = true;
    //        slowTime.RaiseSlowTimeEnter(playerData.SlowAmount);
    //        Debug.Log($"Entered switchTime of: {playerData.Switchtime} seconds");

    //        // Start both tasks with the shared cancellation token
    //        var bowTask = WaitUntilBowVertical(cts.Token); // Pass token to make it cancellable
    //        var delayTask = UniTask.Delay(TimeSpan.FromSeconds(playerData.Switchtime), cancellationToken: cts.Token).AsTask();

    //        // Wait for any task to complete
    //        var completedTask = await Task.WhenAny(bowTask, delayTask);

    //        if (completedTask == bowTask && await bowTask)
    //        {
    //            Debug.Log($"turned bow vertical in the air -> return true");
    //            cts.Cancel(); // Cancel delay task
    //            return true;
    //        }
    //        else
    //        {
    //            Debug.Log("Time ran out -> return false");
    //            cts.Cancel(); // Cancel bow task
    //            return false;
    //        }
    //    }
    //    //catch (OperationCanceledException)
    //    //{
    //    //    // Optional: can be used for logging
    //    //    Debug.Log("One of the tasks was cancelled.");
    //    //    return false;
    //    //}
    //    finally
    //    {
    //        GetComponent<PlayerUIManager>().rotateBowPanel.SetActive(false);
    //        slowTime.RaiseSlowTimeExit();
    //        switchTimeActive = false;
    //        cts.Dispose();
    //    }
    //}



    //[PandaTask]
    //public async Task<bool> SlowTime()
    //{
    //    try
    //    {
    //        if (usedJumpPad)
    //        {
    //            // Explicitly await completion of entire jump pad sequence
    //            await JumpPadSlowtime().ConfigureAwait(false);
    //        }
    //        else if (didLaunchingBash)
    //        {
    //            // Await full completion of bash slowtime
    //            await LaunchingBashSlowtime().ConfigureAwait(false);
    //        }

    //        return true;
    //    }
    //    finally
    //    {
    //        // Reset state flags regardless of completion path
    //        usedJumpPad = false;
    //        didLaunchingBash = false;
    //    }
    //}

    [PandaTask]
    public bool StartSlowTime()
    {
        slowTime.RaiseSlowTimeEnter(playerData.SlowAmount);
        return true;
    }
    [PandaTask]
    public bool StopSlowTime()
    {
        slowTime.RaiseSlowTimeExit();
        return true;
    }

    private async UniTaskVoid JumpPadSlowtime()
    {
        slowTime.RaiseSlowTimeEnter(playerData.SlowAmount); //enter slowtime

        await UniTask.WaitForSeconds(playerData.SlowtimeFromJumppad + 3f/*decceleration duration*/); //wait for slowtime duration
       
        //slowTime.RaiseSlowTimeExit(); //exit slowtime

        levelEvents.RaiseLevelOneWin(); //Raise level win


        //using var cts = new CancellationTokenSource();
        //try
        //{
        //    float totalSlowTime = playerData.SlowtimeFromJumppad + player.SlowTime;
        //    slowTime.RaiseSlowTimeEnter(playerData.SlowAmount);
        //    Debug.Log($"Entered Jumppad slowtime of {totalSlowTime} seconds");

        //    // Create both tasks but don't start them yet
        //    var landTask = WaitUntilGrounded();
        //    var delayTask = Task.Delay(TimeSpan.FromSeconds(totalSlowTime), cts.Token);

        //    // Run both tasks concurrently
        //    var completedTask = await Task.WhenAny(landTask, delayTask);

        //    // Cancel remaining task if needed
        //    if (!completedTask.IsCompleted)
        //    {
        //        cts.Cancel();
        //        await Task.WhenAll(landTask, delayTask).ConfigureAwait(false); // Ensure clean cancellation
        //    }

        //    return true;
        //}
        //catch (OperationCanceledException)
        //{
        //    // Normal cancellation pathway
        //    return true;
        //}
        //finally
        //{
        //    // Ensure cleanup happens before returning
        //    slowTime.RaiseSlowTimeExit();
        //    player.SlowTime = 0f;
        //}
    }

    async UniTaskVoid AfterStartPressed()
    {
        //if the bow is already horizontal, start, if not display message

        await UniTask.WaitForSeconds(1.5f);
        if (Mathf.Abs(GetBowTiltAngle() - horAngleTarget) >= tolerance) //bow not horizontal
        {
            GetComponent<PlayerUIManager>().DisplaySwitchMessage();
            await WaitUntilBowHorizontal();
            GetComponent<PlayerUIManager>().RemoveSwitchMessage();
        }
                

        
        driveControls.enabled = true;
        SetState(PlayerState.Driving);

        await UniTask.WaitForSeconds(2f);

        offRoad.enabled = true;
    }

    public void OnGameEnd()
    {
        offRoad.enabled = false;
        //GetComponent<Rigidbody>().useGravity = false;
        //Debug.Log("gravity set to off");
    }

    private async Task<bool> LaunchingBashSlowtime()
    {
        try
        {
            Debug.Log($"Entered bash slowtime of {playerData.SlowtimeFromBash} seconds");
            slowTime.RaiseSlowTimeEnter(playerData.SlowAmount);

            // Use proper cancellation handling
            await Task.Delay(TimeSpan.FromSeconds(playerData.SlowtimeFromBash))
                .ConfigureAwait(false);

            return true;
        }
        finally
        {
            slowTime.RaiseSlowTimeExit();
        }
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
        PandaTask.Succeed();
    }

    [PandaTask]
    public async Task<bool> WaitUntilGrounded()
    {
        while (!isGrounded)
        {
            await Task.Yield();
        }
        return true;
    }
    [PandaTask]
    public async Task<bool> WaitUntilJump()
    {
        if (!isGrounded)
        {
            return true;
        }
        await UniTask.WaitUntil(() => !isGrounded);
        return true;
    }

    [PandaTask]
    public void DisableArrows()
    {
        bowControls.canShoot = false;
        PandaTask.Succeed();
    }

    [PandaTask]
    public async Task<bool> WaitUntilSwitchtime()
    {
        await UniTask.WaitUntil(() => canEnterSwitchtime);
        canEnterSwitchtime = false;
        Debug.Log("Disabled canEnterSwitchTime");
        return true;
    }

    [PandaTask]
    public bool SwitchTimeActivated()
    {
        if (canEnterSwitchtime)
        {
            canEnterSwitchtime = false;
            Debug.Log("Disabled canEnterSwitchTime");
            return true;
        }
        return false;
    }

    [PandaTask]
    public async Task<bool> WaitUntilBowVertical()
    {
        if (Mathf.Abs(GetBowTiltAngle() - vertAngleTarget) <= tolerance)
        {
            return true;
        }

        await UniTask.WaitUntil(() =>
            Mathf.Abs(GetBowTiltAngle() - vertAngleTarget) <= tolerance);

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
