using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowShooting : ShootingMode
{
    [Header("Arrow & Shooting")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float maxShootForce = 50f;
    public InputActionAsset inputActions;
    private Vector3 storedDirection;
    [SerializeField] private bool fullCharge = false;

    [Header("Calibration")]
    public float minCalibration = 0f;
    public float maxCalibration = 1f;

    [Header("Hand Transforms")]
    public Transform leftHand;   // Aiming hand (holds the bow)
    public Transform rightHand;  // Drawing hand (pulls the string)
    [SerializeField] private Vector3 bowOffsetEuler; // Optional rotation offset

    [Header("Draw Settings")]
    public float maxDrawDistance = 0.5f;
    [SerializeField] private float speedToFire = 0.3f;
    [SerializeField] private float shootCooldown = 0.5f;
    private float shootTimer = 0f;
    [SerializeField] private float delta = 0f;
    [SerializeField] private float minimumTension = 0.4f;

    [Header("Trajectory Preview")]
    public LineRenderer trajectoryLine;
    public int trajectorySteps = 30;
    public float timeStep = 0.1f;
    private Vector3 shootDirection;

    [Header("Animation")]
    public Animator bowAnimator;
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

    [Header("Bowstring")]
    public LineRenderer bowstringLine;
    public Transform stringTop;
    public Transform stringBottom;

    [Header("Arrow Visual")]
    public GameObject arrowVisualPrefab;
    private GameObject currentVisualArrow;
    public Transform arrowNockPoint;

    [Header("Scripts & Events")]
    [SerializeField] private SlowTimeSO slowTimeEvent;
    [SerializeField] private DriveControls driveControls;
    [SerializeField] private StateController stateController;
    [SerializeField] private BluetoothReader btReader;

    private InputAction triggerAction;
    [SerializeField] private float currentFlexValue = 0f;
    public float rawFlex = 0f;
    [SerializeField] private float previousFlexValue = 0f;
    [SerializeField] private bool isDrawing = true;
    private Vector3 drawStartPosition;
    private Player playerScript;

    [Header("Debug Flex Override")]
    public bool useInspectorSlider = false;

    [Range(0f, 1f)]
    public float inspectorFlexValue = 0f;

    void OnEnable()
    {
        playerScript = GetComponent<Player>();
    }

    void Update()
    {
        // Get current flex value either from inspector or Bluetooth
        currentFlexValue = useInspectorSlider ? inspectorFlexValue : 1f - btReader.sensorValue;

        // Update bowstring visual

        UpdateBowstring();

        // Calculate flex change
        delta = currentFlexValue - previousFlexValue;

        // Start drawing if tension starts increasing
        if (!isDrawing && currentFlexValue > 0.05f)
            isDrawing = true;

        if (isDrawing)
        {
            // Show trajectory preview if enough tension
            if (previousFlexValue > 0.05f && !trajectoryLine.enabled)
                trajectoryLine.enabled = true;

            // Calculate shooting direction
            Quaternion shootRotation = leftHand.rotation * Quaternion.Euler(bowOffsetEuler);
            shootDirection = shootRotation * Vector3.forward;

            // Compute preview force and velocity
            float previewForce = previousFlexValue * maxShootForce;
            Vector3 launchVel = shootDirection * previewForce;

            // Draw the trajectory prediction
            DrawTrajectory(shootPoint.position, launchVel);
            Debug.DrawRay(shootPoint.position, launchVel.normalized * 0.5f, Color.red);

            // Store full charge direction
            if (previousFlexValue > 0.95f)
            {
                storedDirection = shootDirection;
                fullCharge = true;
            }
        }

        if (isDrawing && currentFlexValue > 0.05f)
        {
            BowVisualEvents.OnChargeLevelChanged?.Invoke(currentFlexValue);
        }
        else
        {
            BowVisualEvents.OnBowIdle?.Invoke();
        }


        // Check for release condition
        if (isDrawing && delta <= speedToFire && Time.time >= shootTimer && previousFlexValue > minimumTension)
        {
            ReleaseArrow();
            shootTimer = Time.time + shootCooldown;
        }

        // Hide trajectory if not drawing anymore
        if ((!isDrawing || previousFlexValue < 0.05f) && trajectoryLine.enabled)
        {
            trajectoryLine.enabled = false;
        }

        previousFlexValue = currentFlexValue;
    }

    void ReleaseArrow()
    {
        if (!isDrawing) return;


        // Fire the arrow
        Shoot();

        BowVisualEvents.OnArrowReleased?.Invoke();


        // Reset drawing state
        isDrawing = false;
        fullCharge = false;
        currentFlexValue = 0f;
        previousFlexValue = 0f;

        // Hide trajectory
        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        // Update bowstring (return to neutral)
        if (bowstringLine != null)
            UpdateBowstring();

        // Remove visual arrow
        if (currentVisualArrow != null)
        {
            Destroy(currentVisualArrow);
            currentVisualArrow = null;
        }

        // Trigger animation
        if (bowAnimator != null)
        {
            bowAnimator.speed = 1f;
            bowAnimator.SetTrigger(ShootTrigger);
        }

        Debug.Log("Arrow released.");
    }

    void Shoot()
    {
        // Decrease arrow count
        playerScript.ArrowCount--;

        // Calculate shoot direction and force
        float shootForce = currentFlexValue * maxShootForce;
        Quaternion shootRotation = leftHand.rotation * Quaternion.Euler(bowOffsetEuler);
        shootDirection = shootRotation * Vector3.forward;

        // Instantiate arrow
        GameObject arrowObj = Instantiate(projectilePrefab, shootPoint.position, shootRotation);
        Arrow arrow = arrowObj.GetComponent<Arrow>();

        // Launch with full or partial force
        if (fullCharge)
        {
            if (arrow != null)
                arrow.Launch(storedDirection, maxShootForce, 1f);
        }
        else
        {
            if (arrow != null)
                arrow.Launch(shootDirection, shootForce, currentFlexValue);
        }

        // Play animation
        if (bowAnimator != null)
        {
            bowAnimator.speed = 1f;
            bowAnimator.SetTrigger(ShootTrigger);
        }
    }

    void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        trajectoryLine.positionCount = trajectorySteps;
        bool ignoreGravity = previousFlexValue >= 0.9f;

        for (int i = 0; i < trajectorySteps; i++)
        {
            float t = i * timeStep;
            Vector3 point = ignoreGravity
                ? startPos + startVel * t
                : startPos + startVel * t + 0.5f * Physics.gravity * (t * t);

            trajectoryLine.SetPosition(i, point);
        }
    }

    void UpdateBowstring()
    {
        // Define top and bottom of string
        bowstringLine.positionCount = 3;
        bowstringLine.SetPosition(0, stringTop.position);
        bowstringLine.SetPosition(2, stringBottom.position);

        Vector3 middlePoint;

        if (isDrawing)
        {
            // Calculate midpoint and pull direction based on bow rotation
            Vector3 midpoint = (stringTop.position + stringBottom.position) * 0.5f;
            Vector3 zDirection = (leftHand.rotation * Quaternion.Euler(bowOffsetEuler)) * Vector3.back;

            float maxOffset = 0.5f;
            Vector3 offset = zDirection * (currentFlexValue * maxOffset);
            middlePoint = midpoint + offset;
        }
        else
        {
            // Neutral string position
            middlePoint = (stringTop.position + stringBottom.position) * 0.5f;
        }

        bowstringLine.SetPosition(1, middlePoint);
    }
}
