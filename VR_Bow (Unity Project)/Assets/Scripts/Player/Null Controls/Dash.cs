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

    private Rigidbody rb;
    private NullPlayer player;
    private BowMode bowMode;
    private Vector3 dashDirection;
    private float dashStartTime;
    private float dashEndTime;
    private float nextDashAvailableTime;
    private bool controllerWasAboveDashSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 50f;

        player = GetComponent<NullPlayer>();
        bowMode = GetComponent<BowMode>();
    }

    private void Update()
    {
        UpdateControllerData();
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
            StartDash(now);
        }
    }

    private void StartDash(float now)
    {
        // nextDashTime is the full movement duration; cooldown is tracked separately.
        dashStartTime = now;
        dashEndTime = now + nextDashTime;
        nextDashAvailableTime = now + Mathf.Max(nextDashTime, pData.BashCooldown);

        // Begin in the controller's world-space pointing direction, not the body's forward direction.
        dashDirection = bowMode.DashController.forward.normalized;
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
        // Sample only while dashing mode is active. Shooting leaves this direction untouched.
        dashDirection = bowMode.DashController.forward.normalized;
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
