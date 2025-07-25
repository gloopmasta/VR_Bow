using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

[RequireComponent(typeof(Player))]
public class DriveControls : MonoBehaviour, ITimeScalable
{
    private Rigidbody rb;

    [Header("Speed Settings")]
    public float currentSpeed = 5f;
    public Vector3 currentVelocity = Vector3.zero;
    [SerializeField] float maxSpeed = 100f;
    [Range(1.002f, 2f)]
    [SerializeField] float accelerationRate = 1.005f;
    public bool canDrive;

    [SerializeField] float rotOffset = 90f;

    [Header("Bash Settings")]
    [SerializeField] float bashStrength = 5f;
    private float nextBashTime = 0f;


    [Header("Jump Settings")]
    [SerializeField] float jumpStrength = 5f;
    [SerializeField] float jumpCooldown = 0.5f;
    private float nextJumpTime = 0f;

    [Header("Steering & Drift")]
    [SerializeField] bool isDrifting = false;
    [SerializeField] float turnSpeed = 1f;
    [SerializeField] float currentTurnSpeed = 1f;
    [SerializeField] float driftSpeed;
    [SerializeField] float driftCooldown;
    private float nextDriftTime = 0f;
    private float currentSteeringInput = 0f;
    [SerializeField] float steeringSmoothness = 5f;
    [SerializeField] float deadZone = 20f;
    private float lastSteeringInput;
    [SerializeField] private float pitch;

    [Header("Scripts & References")]
    [SerializeField] JumpEventsSO jumpEvents;
    [SerializeField] XRControllerData controllerData;
    [SerializeField] GroundCheck groundCheck;
    [SerializeField] PlayerDataSO pData;
    [SerializeField] LevelEventsSO levelEvents;
    [SerializeField] GameSettings settings;
    [SerializeField] RumbleManager rumble;
    private Player player;

    // ITimeScalable
    public float timeScale = 1f;

    //steering
    private InputDevice controller;

    private void Awake()
    {
        

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 50f;

        player = GetComponent<Player>();

        groundCheck = GetComponent<GroundCheck>();
    }
     
    private void OnEnable()
    {
        //controller right or left - doesn't really work yet.
        controller = settings.rightSteeringController ?
            controllerData._rightController : controllerData._leftController;

        Debug.Log("controller: " + controller.name);

        if (settings.rightSteeringController)
        {
            controller = controllerData._rightController;
            Debug.Log("right controller chosen");
        }
        else
        {
            controller = controllerData._leftController;
            Debug.Log("left controller chosen");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.Register(this);

        levelEvents.OnLevelOneWin += () => 
        {
            Debug.Log("levelOneWin called from DriveControls, disabling gravity");
            OnTimeScaleChanged(0f); //lock movement for player
            rb.useGravity = false; //disable gravity
            rb.isKinematic = true;
            //rb.velocity = Vector3.zero; //disable movement
        };
        levelEvents.OnLevelOneLose += () =>
        {
            Debug.Log("levelOneLose called from DriveControls, disabling gravity");
            OnTimeScaleChanged(0f); //lock movement for player
            rb.useGravity = false; //disable gravity
            rb.isKinematic = true;
            //rb.velocity = Vector3.zero; //disable movement
        };
        levelEvents.OnLevelOneStart += () =>
        {
            OnTimeScaleChanged(1f); // reset movement for player
            rb.useGravity = true; //disable gravity
            rb.isKinematic = false;
        };
        levelEvents.OnLevelOneRestart += () =>
        {
            OnTimeScaleChanged(1f); // reset movement for player
            rb.useGravity = true; //disable gravity
            rb.isKinematic = false;
        };
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Unregister(this);

        levelEvents.OnLevelOneWin -= () => OnTimeScaleChanged(1f); //lock movement for player
        levelEvents.OnLevelOneStart -= () => OnTimeScaleChanged(1f);
        
    }

    public void OnTimeScaleChanged(float newScale)
    {
        timeScale = newScale;
    }

    private void Update()
    {
        float unscaledDelta = Time.fixedDeltaTime * timeScale;

        Drive(unscaledDelta);

        if (canDrive)
        {
            UpdateControllerData();
            Steer(unscaledDelta);
            Drift();
        }

        rb.MovePosition(rb.position + currentVelocity * unscaledDelta);
    }

    private void UpdateControllerData()
    {
        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);

        if (activeController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            //Debug.Log("controller velocity data: " + leftVelocity.ToString());
            float now = Time.time * timeScale;
            if (leftVelocity.z >= 2.5f && now >= nextBashTime)
            {
                Bash();
                nextBashTime = now + pData.BashCooldown;
            }
            if (leftVelocity.y >= 1.5f)
            {
                Debug.Log("controller velocity up enough to jump");
            }
            if (leftVelocity.y >= 1.5f && now >= nextJumpTime)
            {
                Debug.Log("controller velocity up  AND nextjumptime goed");
            }
            if (leftVelocity.y >= 1.5f  && CanJump())
            {
                Debug.Log("controller velocity up  AND and canjump true");
            }
            if (leftVelocity.y >= 1.5f && now >= nextJumpTime && CanJump())
            {
                Debug.Log("player should jump here");
                Jump();
                rumble.RumbleBurst(1f, 0.5f).Forget();
                nextJumpTime = now + jumpCooldown;
            }
        }
    }

    void Bash()
    {
        rb.AddForce(transform.forward * bashStrength, ForceMode.Impulse);
        MakeBashActive().Forget();
    }
    
    private async UniTaskVoid MakeBashActive()
    {
        player.isBashing = true;
        await UniTask.WaitForSeconds(pData.BashActiveTime); //make bash active for however long it is in settings
        player.isBashing = false;
    }

    bool CanJump()
    {
        GroundCheck gc = GetComponentInChildren<GroundCheck>();
        Debug.Log("Player can jump: " + gc.IsGrounded());
        return gc != null && gc.IsGrounded();
    }

    void Jump()
    {
        rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
        jumpEvents.RaiseJump();
    }

    public void Launch(float launchHeight, float accelerationDuration)
    {
        //completely stop velocity to ensure consistent launch height

        //rb.velocity = Vector3.zero;
        //rb.AddForce(transform.up * launchStrength, ForceMode.Impulse);
        //rb.AddForce(transform.forward * 200, ForceMode.Impulse);

        //Custom launch
        //CustomPhysics.MoveWithDecelerationAndSlowPhase(rb, transform.up, (launchHeight / accelerationDuration) * 3f, pData.SlowAmount * 10, accelerationDuration, player.SlowTime - accelerationDuration).Forget();
        CustomPhysics.MoveWithDecelerationAndSlowPhase(rb, transform.up, (launchHeight / accelerationDuration), pData.SlowAmount, 3f, pData.SlowtimeFromJumppad).Forget();
    }

    void Drive(float delta)
    {
        // //slopes
        // Vector3 moveDirection = groundCheck.IsOnSlope()
        //? groundCheck.GetSlopeMoveDirection(transform.forward)
        //: transform.forward;

        //start rumble
        //if (currentVelocity == Vector3.zero) { rumble.StartEngineRumble(0.1f, 1f, 3f).Forget(); }

        //if below max speed -> accelerate
        if (currentVelocity.magnitude < maxSpeed)
        {
            Vector3 acceleration = transform.forward * currentSpeed;
            if (currentVelocity == Vector3.zero)
                currentVelocity = acceleration;
            else
            {
                currentVelocity += acceleration * (accelerationRate - 1f);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);
            }
        }
    }

    private void Drift()
    {
        if (isDrifting) { HandleDrifting(); return; }

        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);

        if (activeController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            float now = Time.time * timeScale;
            if (Mathf.Abs(leftVelocity.x) >= 2.5f && now >= nextDriftTime)
            {
                isDrifting = true;
                nextDriftTime = now + driftCooldown;
            }
        }
    }

    private void HandleDrifting()
    {
        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);

        if (activeController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            if (Mathf.Abs(pitch) > deadZone + rotOffset)
                currentTurnSpeed = driftSpeed;
            else
            {
                currentTurnSpeed = turnSpeed;
                isDrifting = false;
            }
        }
    }

    void Steer(float delta)
    {
        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);

        if (activeController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            Vector3 euler = deviceRotation.eulerAngles;
            pitch = euler.x + rotOffset;
            float absPitch = Mathf.Abs(pitch);

            // Calculate target steering input (0 when in dead zone, otherwise based on pitch)
            float targetInput = (absPitch > deadZone + rotOffset)
                ? Mathf.DeltaAngle(0f, pitch)
                : 0f;

            // Smoothly interpolate steering input
            currentSteeringInput = Mathf.Lerp(
                currentSteeringInput,
                targetInput,
                delta * steeringSmoothness
            );

            // Only apply rotation if we have meaningful input. check prevents tiny residual rotations when nearly centered
            if (absPitch > deadZone + rotOffset || Mathf.Abs(currentSteeringInput) > 0.1f)
            {
                float rotationAmount = currentSteeringInput * currentTurnSpeed * delta;
                Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);

                // Apply rotation to rigidbody
                rb.MoveRotation(rb.rotation * deltaRotation);

                // Rotate velocity to match new direction
                currentVelocity = deltaRotation * currentVelocity;
            }
            else
            {
                // When fully in dead zone, align velocity perfectly with forward
                currentVelocity = transform.forward * currentVelocity.magnitude;
            }
        }

        rb.angularVelocity = Vector3.zero; //reset angular velocity to reduce unwanted spinning
    }

    void Steer()
    {
        if (controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            Vector3 eulerRotation = deviceRotation.eulerAngles;

            // Convert eulerRotation.x from 0–360 to -180–180
            pitch = eulerRotation.x + rotOffset;

            //.Log("Pitch: "+pitch);

            // Check if the adjusted pitch is outside the dead zone -> then rotate
            if (Mathf.Abs(pitch) > deadZone + rotOffset) // Ansolute to check both - and +, chezck the absolute value ifg it's minus basically
            {
                // Calculate steering input
                float targetSteeringInput = Mathf.DeltaAngle(0f, pitch);

                // Smoothly interpolate between current and target steering input
                currentSteeringInput = Mathf.Lerp(currentSteeringInput, targetSteeringInput, Time.deltaTime * steeringSmoothness);

                // Apply rotation - OLD
                //transform.Rotate(Vector3.up, currentSteeringInput * currentTurnSpeed * Time.deltaTime);

                //apply rotation - NEW
                float rotationAmount = currentSteeringInput * currentTurnSpeed * Time.fixedDeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
                rb.MoveRotation(rb.rotation * deltaRotation);


                lastSteeringInput = currentSteeringInput;
            }
            else
            {
                // Within dead zone; smoothly return to zero
                currentSteeringInput = Mathf.Lerp(lastSteeringInput, 0f, Time.deltaTime * steeringSmoothness);

                //transform.Rotate(Vector3.up, currentSteeringInput * turnSpeed * Time.deltaTime);
            }
        }
    }




    //public void TriggerHapticFeedback()
    //{
    //    InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    //    if (leftController.isValid)
    //        leftController.SendHapticImpulse(0, 1f, 0.5f);
    //}
}
//For even smoother transitions:

//// Replace the else block with:
//else if (currentSteeringInput != 0f)
//{
//    // Gradually align to forward when entering dead zone
//    currentVelocity = Vector3.RotateTowards(
//        currentVelocity,
//        transform.forward * currentVelocity.magnitude,
//        delta * steeringSmoothness * Mathf.Deg2Rad,
//        0f
//    );
//}
//Consider adding a small dead zone buffer to prevent rapid toggling at the threshold:

//csharp
//float deadZoneBuffer = 5f; // Degrees of hysteresis
//bool wasOutsideDeadzone = absPitch > (deadZone + rotOffset);
//bool isOutsideDeadzone = absPitch > (deadZone + rotOffset - deadZoneBuffer);

//// Use isOutsideDeadzone for checks but wasOutsideDeadzone for transitions