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
        currentFlexValue = 1f - btReader.sensorValue; // Ensure this is between 0 and 1
        UpdateBowstring();


        if (!isDrawing && currentFlexValue > 0.05f)
        {
            isDrawing = true;
            // Additional setup if needed
        }


        delta = currentFlexValue - previousFlexValue;

        if (delta >= speedToFire)
            Debug.Log("velocity: " + delta);

        
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
        Vector3 shootDirection = shootRotation * Vector3.forward;
        GameObject arrowObj = Instantiate(projectilePrefab, shootPoint.position, shootRotation);

        Arrow arrow = arrowObj.GetComponent<Arrow>();
        if (arrow != null)
            arrow.Launch(shootDirection, shootForce, currentFlexValue);

        if (bowAnimator != null)
        {
            bowAnimator.speed = 1f;
            bowAnimator.SetTrigger(ShootTrigger);
        }
    }

    void DrawTrajectory(Vector3 startPos, Vector3 startVel)
    {
        if (trajectoryLine == null) return;

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
        if (bowstringLine == null || stringTop == null || stringBottom == null) return;

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
