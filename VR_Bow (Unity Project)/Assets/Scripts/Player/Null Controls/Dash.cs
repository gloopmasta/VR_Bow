using UnityEngine;
using UnityEngine.XR;

public class Dash : MonoBehaviour
{
    [SerializeField] XRControllerData controllerData;
    [SerializeField] GameSettings settings;
    [SerializeField] PlayerDataSO pData;
    [SerializeField, Min(0.01f)] float nextDashTime = 5f;
    [SerializeField] float dashStrength = 500f;
    [SerializeField, Min(0.01f)] float dashTriggerSpeed = 2.5f;
    [SerializeField] RumbleManager rumble;

    [Header("Horizontal Dash Steering")]
    // These existing settings turn the player around the Y axis during a dash.
    [SerializeField] float rotOffset = 90f;
    [SerializeField, Min(0f)] float steeringDeadZone = 8f;
    [SerializeField, Min(0f)] float steeringSmoothness = 5f;
    [SerializeField, Min(0f)] float maxTurnSpeed = 4f;
    [SerializeField, Min(0f)] float maxDashTurn = 5f;

    [Header("Vertical Dash Steering")]
    [SerializeField] Transform headTransform;
    [SerializeField] Transform leftControllerTransform;
    [SerializeField] Transform rightControllerTransform;
    [SerializeField, Min(0f)] float shoulderDrop = 0.25f;
    [SerializeField, Min(0f)] float minimumArmReach = 0.4f; // Ignore hands held close to the body, where the arm angle is unreliable.
    [SerializeField, Range(0f, 89f)] float verticalDeadZone = 12f;
    // Arm angles at or beyond this value request the maximum dash pitch.
    [SerializeField, Range(0f, 89f)] float fullSteeringAngle = 45f;
    // Maximum upward or downward travel angle, in degrees.
    [SerializeField, Range(0f, 45f)] float maxDashPitch = 10f;
    // Maximum number of degrees the dash pitch can change per second.
    [SerializeField, Min(0f)] float pitchSteeringSpeed = 30f;

    private Rigidbody rb;
    private NullPlayer player;
    private BowMode bowMode;
    private Vector3 dashDirection; // Actual unit travel direction, combining the current horizontal heading and pitch.
    private Vector3 horizontalDashDirection; // Unit heading on the ground plane; existing yaw steering rotates this vector.
    private float dashStartTime;
    private float dashEndTime;
    private float nextDashAvailableTime;
    private bool controllerWasAboveDashSpeed;
    private float currentSteeringInput;
    private float dashTurn;
    // Smoothed vertical angle used to avoid a sudden change when the hand moves.
    private float currentDashPitch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 50f;

        player = GetComponent<NullPlayer>();
        bowMode = GetComponent<BowMode>();
        Transform hand = GetSteeringHand();
        if (headTransform == null || hand == null)
            Debug.LogWarning($"Dash vertical steering references: head={(headTransform != null ? headTransform.name : "missing")}, " +
                $"hand={(hand != null ? hand.name : "missing")}. Check the active Player rig.", this);
    }

    private void Update()
    {
        UpdateControllerData();
        Debug.Log("Target dash pitch: " + GetTargetDashPitch());
    }

    private void FixedUpdate()
    {
        if (!player.IsDashing) return;

        // Use physics time for both the speed curve and Rigidbody velocity changes.
        if (Time.time >= dashEndTime)
        {
            StopDash();
            return;
        }

        // Shooting locks in the last travel direction, but the speed curve continues.
        if (bowMode != null && bowMode.State == PlayerState.Dashing)
            Steer();

        PropelForward();
    }

    private void OnDisable()
    {
        // An interrupted dash must not leave the player marked as dashing.
        if (player != null && player.IsDashing)
            StopDash();

        controllerWasAboveDashSpeed = false;
    }

    private void UpdateControllerData()
    {
        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);
        if (!activeController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 controllerVelocity))
        {
            controllerWasAboveDashSpeed = false;
            return;
        }

        // Squared 3D speed includes +X, -X, +Y, -Y, +Z and -Z without six checks.
        bool aboveDashSpeed = controllerVelocity.sqrMagnitude >= dashTriggerSpeed * dashTriggerSpeed;
        bool newBurst = aboveDashSpeed && !controllerWasAboveDashSpeed;
        controllerWasAboveDashSpeed = aboveDashSpeed;

        // Only the crossing starts a dash, so holding a fast motion cannot retrigger it.
        float now = Time.time;
        if (newBurst && bowMode != null && bowMode.isActiveAndEnabled &&
            bowMode.DashController != null && !player.IsDashing && !rb.isKinematic &&
            bowMode.State == PlayerState.Dashing && now >= nextDashAvailableTime)
        {
            Debug.Log("START DASH");
            StartDash(now, controllerVelocity);
        }
    }

    private void StartDash(float now, Vector3 controllerVelocity)
    {
        // nextDashTime is the full movement duration; cooldown is tracked separately.
        dashStartTime = now;
        dashEndTime = now + nextDashTime;
        nextDashAvailableTime = now + Mathf.Max(nextDashTime, pData.BashCooldown);

        // XR device velocity is relative to the tracking origin. In this rig,
        // the controller's parent provides the tracking-space orientation.
        Transform trackingSpace = bowMode.DashController.parent;
        Vector3 worldVelocity = trackingSpace != null
            ? trackingSpace.TransformDirection(controllerVelocity)
            : controllerVelocity;

        // The hand swing chooses the horizontal heading. Arm elevation controls
        // the small vertical component independently of the swing direction.
        horizontalDashDirection = Vector3.ProjectOnPlane(worldVelocity, Vector3.up);
        // An almost vertical swing has no useful heading; use the player's view direction.
        if (horizontalDashDirection.sqrMagnitude < 0.0001f)
            horizontalDashDirection = Vector3.ProjectOnPlane(
                headTransform != null ? headTransform.forward : transform.forward, Vector3.up);
        // Looking straight up or down still needs a valid horizontal fallback.
        if (horizontalDashDirection.sqrMagnitude < 0.0001f)
            horizontalDashDirection = transform.forward;
        horizontalDashDirection.Normalize();

        // Start each dash with a fresh yaw limit and the hand's current arm elevation.
        currentSteeringInput = 0f;
        dashTurn = 0f;
        currentDashPitch = GetTargetDashPitch();
        UpdateDashDirection();
        rb.velocity = Vector3.zero;
        player.IsDashing = true;
    }

    private void StopDash()
    {
        rb.velocity = Vector3.zero;
        player.IsDashing = false;
    }

    private void PropelForward()
    {
        float progress = Mathf.Clamp01((Time.time - dashStartTime) / nextDashTime);
        float speedFactor;

        // Rise quickly, cruise at full speed, then ease toward zero over the final 40%.
        if (progress < 0.12f)
            speedFactor = Mathf.SmoothStep(0f, 1f, progress / 0.12f);
        else if (progress < 0.6f)
            speedFactor = 1f;
        else
            speedFactor = 1f - BezierEase((progress - 0.6f) / 0.4f);

        // The old impulse produced dashStrength / mass metres per second. Keep that peak speed.
        rb.velocity = dashDirection * (dashStrength / rb.mass) * speedFactor;
    }

    private void Steer()
    {
        // Apply the existing left/right turn, then independently update elevation.
        SteerHorizontally();
        currentDashPitch = Mathf.MoveTowards(currentDashPitch, GetTargetDashPitch(),
            pitchSteeringSpeed * Time.fixedDeltaTime);
        UpdateDashDirection();
    }

    private void SteerHorizontally()
    {
        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);
        if (!activeController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            currentSteeringInput = 0f;
            return;
        }

        // Keep the existing yaw steering driven by the controller pitch.
        float pitch = Mathf.DeltaAngle(0f, deviceRotation.eulerAngles.x + rotOffset);
        float pitchPastDeadZone = Mathf.Max(0f, Mathf.Abs(pitch) - steeringDeadZone);
        float targetInput = Mathf.Sign(pitch) * Mathf.Clamp01(pitchPastDeadZone / 30f);
        currentSteeringInput = Mathf.Lerp(currentSteeringInput, targetInput,
            Mathf.Clamp01(Time.fixedDeltaTime * steeringSmoothness));

        // Preserve the existing per-second and total yaw limits.
        float nextTurn = Mathf.Clamp(dashTurn + currentSteeringInput * maxTurnSpeed * Time.fixedDeltaTime,
            -maxDashTurn, maxDashTurn);
        float turnAmount = nextTurn - dashTurn;
        dashTurn = nextTurn;
        if (Mathf.Abs(turnAmount) < 0.0001f) return;

        Quaternion turn = Quaternion.Euler(0f, turnAmount, 0f);
        transform.rotation = turn * transform.rotation;
        // Turn the ground-plane heading so the vertical pitch remains independent.
        horizontalDashDirection = turn * horizontalDashDirection;
    }

    private float GetTargetDashPitch()
    {
        // Match the hand chosen by GameSettings for the existing steering input.
        Transform hand = GetSteeringHand();
        if (headTransform == null || hand == null)
            return 0f;

        // Keep the shoulder point under the tracked head so room-scale movement
        // and player height do not change the arm angle.
        Vector3 shoulder = headTransform.position - Vector3.up * shoulderDrop;
        Vector3 arm = hand.position - shoulder;
        // atan2 gives a signed elevation: positive above the shoulder, negative below.
        float horizontalReach = Vector3.ProjectOnPlane(arm, Vector3.up).magnitude;
        float armPitch = Mathf.Atan2(arm.y, horizontalReach) * Mathf.Rad2Deg;
        Debug.Log($"Raw arm pitch: {armPitch:F1}°, reach: {arm.magnitude:F2} m, minimum: {minimumArmReach:F2} m, hand: {hand.name}");
        if (arm.sqrMagnitude < minimumArmReach * minimumArmReach)
            return 0f;

        // Remove the neutral zone, then scale and cap the requested dash pitch.
        float anglePastDeadZone = Mathf.Max(0f, Mathf.Abs(armPitch) - verticalDeadZone);
        float range = Mathf.Max(0.01f, fullSteeringAngle - verticalDeadZone);
        return Mathf.Sign(armPitch) * maxDashPitch * Mathf.Clamp01(anglePastDeadZone / range);
    }

    private Transform GetSteeringHand()
    {
        if (settings == null) return null;
        return settings.rightSteeringController ? rightControllerTransform : leftControllerTransform;
    }

    private void OnDrawGizmosSelected()
    {
        Transform hand = GetSteeringHand();
        if (headTransform == null || hand == null) return;

        // Draw the same shoulder-to-hand vector used to calculate dash pitch.
        Vector3 shoulder = headTransform.position - Vector3.up * shoulderDrop;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(shoulder, hand.position);
        Gizmos.DrawWireSphere(shoulder, 0.04f);
    }

    private void UpdateDashDirection()
    {
        // Recombine yaw and pitch into a unit vector without tilting the player body.
        float radians = currentDashPitch * Mathf.Deg2Rad;
        dashDirection = horizontalDashDirection * Mathf.Cos(radians) +
            Vector3.up * Mathf.Sin(radians);
    }

    private static float BezierEase(float progress)
    {
        // Approximate CSS cubic-bezier(.16, .56, .55, .99) by finding its x parameter.
        float lower = 0f;
        float upper = 1f;
        for (int i = 0; i < 10; i++)
        {
            float t = (lower + upper) * 0.5f;
            float inverse = 1f - t;
            float x = 3f * inverse * inverse * t * 0.16f +
                      3f * inverse * t * t * 0.55f + t * t * t;
            if (x < progress) lower = t;
            else upper = t;
        }

        float parameter = (lower + upper) * 0.5f;
        float remaining = 1f - parameter;
        return 3f * remaining * remaining * parameter * 0.56f +
               3f * remaining * parameter * parameter * 0.99f +
               parameter * parameter * parameter;
    }
}
