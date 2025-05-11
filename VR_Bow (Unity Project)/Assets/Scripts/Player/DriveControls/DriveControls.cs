using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DriveControls : MonoBehaviour
{
    private Rigidbody rb;


    [Header("Speed Settings")]
    public float currentSpeed = 5f;
    [SerializeField] private Vector3 currentVelocity = Vector3.zero;
    [SerializeField] float maxSpeed = 100f;
    [Range(1.002f, 2f)]
    [SerializeField] float accelerationRate = 1.005f;
    public bool canDrive;


    [SerializeField] float rotOffset = 90f;

    [Header("Bash Settings")]
    [SerializeField] float bashStrength = 5f;
    [SerializeField] int bashDamage = 1;
    [SerializeField] private float bashCooldown = 1.5f; // Cooldown duration in seconds
    private float nextBashTime = 0f; // Time when the next bash is allowed

    [Header("Jump Settings")]
    [SerializeField] float jumpStrength = 5f;
    [SerializeField] float jumpCooldown = 0.5f;
    private float nextJumpTime = 0f;

    [Header("Steering & drift")]
    [SerializeField] bool isDrifting = false;
    [SerializeField] float turnSpeed = 1f;
    [SerializeField] float currentTurnSpeed = 1f;
    [SerializeField] float driftSpeed;
    [SerializeField] float driftCooldown;
    private float nextDriftTime = 0f;
    private float currentSteeringInput = 0f;
    [SerializeField] float steeringSmoothness = 5f; // Adjust for desired smoothness
    [SerializeField] float deadZone = 20f;
    private float lastSteeringInput;
    private float pitch;


    [Header("Scripts & References")]
    [SerializeField] XRControllerData controllerData;
    [SerializeField] BoxCollider groundCollider;
    [SerializeField] GameObject lockPoint;
    [SerializeField] GameObject BowMesh;


    //How much the handle is turned
    [SerializeField] float revStrength = 1f;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // ← Critical for VR
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 50f; // ← Avoid ultra-light weights
    }

    private void FixedUpdate()
    {
        Debug.Log(Time.timeScale);
        float unscaledDelta = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);

        Drive(unscaledDelta); //avoid jitter when timeSlow

        if (canDrive)
        {
            UpdateControllerData();
            Steer(unscaledDelta);
            Drift();
            LockBowPosition();
        }

        //move the player useing rigidbody
        rb.MovePosition(rb.position + currentVelocity * unscaledDelta);

    }



    private void UpdateControllerData()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            //BASH CHECK
            if (leftVelocity.z >= 2.5f && Time.time >= nextBashTime) // Swing forward and cooldown check
            {
                Bash();
                TriggerHapticFeedback();
                nextBashTime = Time.time + bashCooldown; // Set next allowable bash time
            }
            //JUMP CHECK
            if (leftVelocity.y >= 2.5f && Time.time >= nextJumpTime && CanJump())
            {
                Debug.Log("JUMPED");
                Jump();
                TriggerHapticFeedback();
                nextJumpTime = Time.time + jumpCooldown;
            }
        }
    }

    void Bash()
    {
        // Use ForceMode.VelocityChange for time-independent forces
        rb.AddForce(transform.forward * bashStrength, ForceMode.VelocityChange);
    }

    bool CanJump()
    {
        GroundCheck gc = GetComponentInChildren<GroundCheck>();
        Debug.Assert(gc != null, "did not find GroundCheckScript in children");

        // If you can jump AND your cooldown is complete
        if (gc != null && gc.IsGrounded())
        {
            return true;
        }

        Debug.Log("player is not grounded");
        return false;
    }
    void Jump()
    {
        // VelocityChange ignores mass and provides immediate response
        rb.AddForce(transform.up * jumpStrength, ForceMode.VelocityChange);
    }

    public void Launch(float launchStrength)
    {
        rb.AddForce(transform.up * launchStrength, ForceMode.Impulse);
    }

    
    void Drive(float delta)
    {
        // Accelerate forward if below max speed
        if (currentVelocity.magnitude < maxSpeed)
        {
            Vector3 acceleration = transform.forward * currentSpeed * delta;

            if (currentVelocity == Vector3.zero)
            {
                // Start with a base forward velocity
                currentVelocity = acceleration;
            }
            else
            {
                // Exponential acceleration
                currentVelocity += acceleration * (accelerationRate - 1f);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);
            }
        }
    }


    private void Drift()
    {
        if (isDrifting)
        {
            HandleDrifting();
            return;
        }

        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            if (Mathf.Abs(leftVelocity.x) >= 2.5f && Time.time >= nextDriftTime) // If swing sideways left or right more than 2.5f velocity
            {
                isDrifting = true;

                Debug.Log("started a drift");
                TriggerHapticFeedback();
                nextDriftTime = Time.time + driftCooldown;
            }
        }
    }

    private void HandleDrifting()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            if (Mathf.Abs(pitch) > deadZone + rotOffset) // outside of deadzone
            {
                currentTurnSpeed = driftSpeed;
            }
            else //within deadzone -> stop drifting
            {
                currentTurnSpeed = turnSpeed;
                isDrifting = false;
            }
        }
    }

    private Vector3 GetControllerRotation()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            return deviceRotation.eulerAngles;
        }

        return Vector3.zero;
    }

    //void Steer()
    //{
    //    if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
    //    {
    //        Vector3 eulerRotation = deviceRotation.eulerAngles;

    //        // Convert eulerRotation.x from 0–360 to -180–180
    //        pitch = eulerRotation.x + rotOffset;

    //        //.Log("Pitch: "+pitch);

    //        // Check if the adjusted pitch is outside the dead zone -> then rotate
    //        if (Mathf.Abs(pitch) > deadZone + rotOffset) // Ansolute to check both - and +, chezck the absolute value ifg it's minus basically
    //        {
    //            // Calculate steering input
    //            float targetSteeringInput = Mathf.DeltaAngle(0f, pitch);

    //            // Smoothly interpolate between current and target steering input
    //            currentSteeringInput = Mathf.Lerp(currentSteeringInput, targetSteeringInput, Time.deltaTime * steeringSmoothness);

    //            // Apply rotation - OLD
    //            //transform.Rotate(Vector3.up, currentSteeringInput * currentTurnSpeed * Time.deltaTime);

    //            //apply rotation - NEW
    //            float rotationAmount = currentSteeringInput * currentTurnSpeed * Time.fixedDeltaTime;
    //            Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
    //            rb.MoveRotation(rb.rotation * deltaRotation);


    //            lastSteeringInput = currentSteeringInput;
    //        }
    //        else
    //        {
    //            // Within dead zone; smoothly return to zero
    //            currentSteeringInput = Mathf.Lerp(lastSteeringInput, 0f, Time.deltaTime * steeringSmoothness);

    //            //transform.Rotate(Vector3.up, currentSteeringInput * turnSpeed * Time.deltaTime);
    //        }
    //    }
    //}
    void Steer(float delta)
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
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


                // Modified rotation with unscaled time
                float rotationAmount = currentSteeringInput * currentTurnSpeed * delta;
                Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
                rb.MoveRotation(rb.rotation * deltaRotation);


                currentSteeringInput = Mathf.Lerp(
                currentSteeringInput,
                targetSteeringInput,
                Time.unscaledDeltaTime * steeringSmoothness);
            }
            else
            {
                // Within dead zone; smoothly return to zero
                currentSteeringInput = Mathf.Lerp(lastSteeringInput, 0f, Time.deltaTime * steeringSmoothness);

                //transform.Rotate(Vector3.up, currentSteeringInput * turnSpeed * Time.deltaTime);
            }
        }
    }

    private void LockBowPosition()
    {
        BowMesh.transform.position = lockPoint.transform.position;
    }

    public void TriggerHapticFeedback()
    {
        // Get the right controller
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (leftController.isValid)
        {
            // Send haptic impulse
            leftController.SendHapticImpulse(0, 1f, 0.5f);
        }
    }

}