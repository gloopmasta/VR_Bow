using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.WSA;

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
    public Transform leftHand;   // Aiming
    public Transform rightHand;  // Drawing
    [SerializeField] private Vector3 bowOffsetEuler;

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

    [Header("Scripts & events")]
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



    void OnEnable()
    {
        playerScript = GetComponent<Player>();
    }


    void Update()
    {
        rawFlex = 1f - btReader.sensorValue; // Ensure this is between 0 and 1

        currentFlexValue = Mathf.Clamp01(Mathf.InverseLerp(minCalibration, maxCalibration, rawFlex)); //calibrate

        UpdateBowstring();


        if (!isDrawing && currentFlexValue > 0.05f)
        {
            isDrawing = true;
            // Additional setup if needed
        }


        delta = currentFlexValue - previousFlexValue;

        if (delta <= -speedToFire)
            Debug.Log("velocity: " + delta);

        if (isDrawing)
        {
            //if (!trajectoryLine.enabled)
            //    trajectoryLine.enabled = true;

            // Calculate current shooting direction while drawing
            Quaternion shootRotation = leftHand.rotation * Quaternion.Euler(bowOffsetEuler);
            shootDirection = shootRotation * Vector3.forward;

            float previewForce = currentFlexValue * maxShootForce;
            Vector3 launchVel = shootDirection * previewForce;

            //draw trajectory
            DrawTrajectory(shootPoint.position, launchVel);
            Debug.DrawRay(shootPoint.position, launchVel.normalized * 0.5f, Color.red);

            //store direction and speed
            if (previousFlexValue > 0.95f)
            {
                storedDirection = shootDirection; //store the direction
                fullCharge = true;
            }
        }
        //else
        //{
        //    if (trajectoryLine.enabled)
        //        trajectoryLine.enabled = false;
        //}


        if (isDrawing && delta <= speedToFire && Time.time >= shootTimer && previousFlexValue > minimumTension)
        {
            ReleaseArrow();
            shootTimer = Time.time + shootCooldown;
        }

        previousFlexValue = currentFlexValue;


        

        //    bowAnimator.speed = 0f;

        //if (bowAnimator != null)
        //{
        //    bowAnimator.Play("Charge", 0, currentFlexValue);
        //    bowAnimator.speed = 0f;
        //}


    }






    void ReleaseArrow()
    {
        if (!isDrawing) return;

        Shoot();
        isDrawing = false;
        fullCharge = false;
        currentFlexValue = 0f;
        previousFlexValue = 0f;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        if (bowstringLine != null)
            bowstringLine.enabled = false;

        if (currentVisualArrow != null)
        {
            Destroy(currentVisualArrow);
            currentVisualArrow = null;
        }

        if (bowAnimator != null)
        {
            bowAnimator.speed = 1f;
            bowAnimator.SetTrigger(ShootTrigger);
        }

        Debug.Log("Arrow released.");
    }


    void Shoot()
    {
        playerScript.ArrowCount--; //deduct an arrow from the player
        float shootForce = currentFlexValue * maxShootForce;
        Quaternion shootRotation = leftHand.rotation * Quaternion.Euler(bowOffsetEuler);
        shootDirection = shootRotation * Vector3.forward;

        GameObject arrowObj = Instantiate(projectilePrefab, shootPoint.position, shootRotation);

        Arrow arrow = arrowObj.GetComponent<Arrow>();

        if (fullCharge)
        {
            if (arrow != null)
                arrow.Launch(storedDirection, 1f * maxShootForce, 1f); //if full charge, shoot fast
        }
        else 
        {
            if (arrow != null)
                arrow.Launch(shootDirection, shootForce, currentFlexValue);
        }

        if (bowAnimator != null)
        {
            bowAnimator.speed = 1f;
            bowAnimator.SetTrigger(ShootTrigger);
        }
    }

    void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        //if (trajectoryLine == null) return;

        trajectoryLine.positionCount = trajectorySteps;
        bool ignoreGravity = currentFlexValue >= 0.9f;

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
        //if (bowstringLine == null || stringTop == null || stringBottom == null) return;

        bowstringLine.positionCount = 3;
        bowstringLine.SetPosition(0, stringTop.position);
        bowstringLine.SetPosition(2, stringBottom.position);

        Vector3 middlePoint;

        if (isDrawing)
        {
            // Calculate the midpoint between stringTop and stringBottom
            Vector3 midpoint = (stringTop.position + stringBottom.position) * 0.5f;

            // Determine the direction of the local Z-axis
            Vector3 zDirection = -transform.forward;

            // Define the maximum offset distance along the Z-axis
            float maxOffset = 0.5f; // Adjust this value as needed

            // Calculate the offset based on currentFlexValue
            Vector3 offset = zDirection * (currentFlexValue * maxOffset);

            // Apply the offset to the midpoint
            middlePoint = midpoint + offset;
        }
        else
        {
            // When not drawing, the middle point is just the midpoint
            middlePoint = (stringTop.position + stringBottom.position) * 0.5f;
        }

        bowstringLine.SetPosition(1, middlePoint);
    }

}
