using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PandaBT;
using PandaBT.Runtime;
using System.Threading.Tasks;

public class StateController : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private Player player;
    [SerializeField] private BowControls bowControls;
    [SerializeField] private DriveControls driveControls;
    [SerializeField] private BowModelHandler bowModelHandler;
    [SerializeField] private JumpEventsSO jumpEvents;
    [SerializeField] private SwitchTimeEventsSO switchEvents;
    [SerializeField] public SlowTimeSO slowTime;

    [Header("Bow Rotation")]
    [SerializeField] private Transform bowTransform;
    [SerializeField] private float vertAngleTarget = 90f;
    [SerializeField] private float horAngleTarget = 0f;
    [SerializeField] private float tolerance = 20f;

    [Header("Aditional Settings")]
    [SerializeField][PandaVariable] public bool isGrounded;
    [SerializeField] private bool canEnterSwitchtime = false;
    [SerializeField] private bool switchTimeActive = false;


    private void OnEnable()
    {
        jumpEvents.OnJump += () => isGrounded = false;
        jumpEvents.OnLand += () => isGrounded = true;
        switchEvents.OnEnterDSSwitchTime += () => canEnterSwitchtime = true;

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
    public async Task<bool> SlowTime(float duration)
    {
        //return await slowTime.SlowTime(duration);

        return true;
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

        if (!switchTimeActive)
        {
            return false;
        }

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
