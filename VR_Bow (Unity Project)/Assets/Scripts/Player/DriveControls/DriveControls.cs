using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DriveControls : MonoBehaviour, ITimeScalable
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
    [SerializeField] private float bashCooldown = 1.5f;
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
    private float pitch;

    [Header("Scripts & References")]
    [SerializeField] XRControllerData controllerData;
    [SerializeField] BoxCollider groundCollider;
    [SerializeField] GameObject lockPoint;
    [SerializeField] GameObject BowMesh;

    // ITimeScalable
    private float timeScale = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 50f;
    }

    private void OnEnable()
    {
        GameManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Unregister(this);
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
            LockBowPosition();
        }

        rb.MovePosition(rb.position + currentVelocity * unscaledDelta);
    }

    private void UpdateControllerData()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            float now = Time.time * timeScale;
            if (leftVelocity.z >= 2.5f && now >= nextBashTime)
            {
                Bash();
                TriggerHapticFeedback();
                nextBashTime = now + bashCooldown;
            }
            if (leftVelocity.y >= 2.5f && now >= nextJumpTime && CanJump())
            {
                Jump();
                TriggerHapticFeedback();
                nextJumpTime = now + jumpCooldown;
            }
        }
    }

    void Bash()
    {
        rb.AddForce(transform.forward * bashStrength, ForceMode.VelocityChange);
    }

    bool CanJump()
    {
        GroundCheck gc = GetComponentInChildren<GroundCheck>();
        return gc != null && gc.IsGrounded();
    }

    void Jump()
    {
        rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
    }

    public void Launch(float launchStrength)
    {
        rb.AddForce(transform.up * launchStrength, ForceMode.Impulse);
    }

    void Drive(float delta)
    {
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

        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            float now = Time.time * timeScale;
            if (Mathf.Abs(leftVelocity.x) >= 2.5f && now >= nextDriftTime)
            {
                isDrifting = true;
                TriggerHapticFeedback();
                nextDriftTime = now + driftCooldown;
            }
        }
    }

    private void HandleDrifting()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
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
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            Vector3 euler = deviceRotation.eulerAngles;
            pitch = euler.x + rotOffset;

            if (Mathf.Abs(pitch) > deadZone + rotOffset)
            {
                float targetInput = Mathf.DeltaAngle(0f, pitch);
                currentSteeringInput = Mathf.Lerp(currentSteeringInput, targetInput, delta * steeringSmoothness);
                float rotationAmount = currentSteeringInput * currentTurnSpeed * delta;
                Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
                rb.MoveRotation(rb.rotation * deltaRotation);
                lastSteeringInput = currentSteeringInput;
            }
            else
            {
                currentSteeringInput = Mathf.Lerp(lastSteeringInput, 0f, delta * steeringSmoothness);
            }
        }
    }

    void Steer()
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



    private void LockBowPosition()
    {
        BowMesh.transform.position = lockPoint.transform.position;
    }

    public void TriggerHapticFeedback()
    {
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftController.isValid)
            leftController.SendHapticImpulse(0, 1f, 0.5f);
    }
}
