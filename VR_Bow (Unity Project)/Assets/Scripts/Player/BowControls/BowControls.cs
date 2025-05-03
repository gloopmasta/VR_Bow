using UnityEngine;
using UnityEngine.InputSystem;

public class BowControls : MonoBehaviour
{
    [Header("Arrow & Shooting")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float maxShootForce = 20f;
    public InputActionAsset inputActions;

    [Header("Hand Transforms")]
    public Transform leftHand;   // Aiming
    public Transform rightHand;  // Drawing

    [Header("Draw Settings")]
    public float maxDrawDistance = 0.5f;

    [Header("Trajectory Preview")]
    public LineRenderer trajectoryLine;
    public int trajectorySteps = 30;
    public float timeStep = 0.1f;

    [Header("Animation")]
    public Animator bowAnimator;
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

    [Header("Bowstring")]
    public LineRenderer bowstringLine;
    public Transform stringTop;    // Top of bow
    public Transform stringBottom; // Bottom of bow

    private InputAction triggerAction;
    private float flexValue = 0f;
    private bool isDrawing = false;
    private Vector3 drawStartPosition;

    void OnEnable()
    {
        var gameplayMap = inputActions.FindActionMap("VrPlayerController");
        triggerAction = gameplayMap.FindAction("RightTrigger");

        gameplayMap.Enable();

        triggerAction.started += _ => StartDrawing();
        triggerAction.canceled += _ => ReleaseArrow();
    }

    void OnDisable()
    {
        triggerAction.started -= _ => StartDrawing();
        triggerAction.canceled -= _ => ReleaseArrow();
        triggerAction.Disable();
    }

    void Update()
    {
        if (isDrawing)
        {
            float drawDistance = Vector3.Distance(drawStartPosition, rightHand.position);
            flexValue = Mathf.Clamp01(drawDistance / maxDrawDistance);

            // Animate charge by scrubbing normalized time
            if (bowAnimator != null)
            {
                bowAnimator.Play("Charge", 0, flexValue); // Make sure this matches your animation state's name
                bowAnimator.speed = 0f;
            }

            // Show trajectory
            Vector3 launchDir = leftHand.forward;
            float previewForce = flexValue * maxShootForce;
            Vector3 launchVel = launchDir * previewForce;

            DrawTrajectory(shootPoint.position, launchVel);
            Debug.DrawRay(shootPoint.position, launchVel.normalized * 0.5f, Color.red);
        }
        else
        {
            if (trajectoryLine != null && trajectoryLine.enabled)
                trajectoryLine.enabled = false;
        }

        // Always update bowstring
        UpdateBowstring();
    }

    void StartDrawing()
    {
        isDrawing = true;
        drawStartPosition = rightHand.position;

        if (trajectoryLine != null)
            trajectoryLine.enabled = true;

        if (bowstringLine != null)
            bowstringLine.enabled = true;

        Debug.Log("Started drawing bow.");
    }

    void ReleaseArrow()
    {
        if (!isDrawing) return;

        Shoot();
        isDrawing = false;
        flexValue = 0f;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        if (bowstringLine != null)
            bowstringLine.enabled = false;

        Debug.Log("Arrow released.");
    }

    void Shoot()
    {
        float shootForce = flexValue * maxShootForce;
        Vector3 shootDirection = leftHand.forward;

        GameObject arrowObj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        if (arrow != null)
            arrow.Launch(shootDirection, shootForce, flexValue);

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
        bool ignoreGravity = flexValue >= 0.9f;

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

        Vector3 middlePoint = isDrawing
            ? rightHand.position
            : Vector3.Lerp(stringTop.position, stringBottom.position, 0.5f);

        bowstringLine.SetPosition(1, middlePoint);
        bowstringLine.SetPosition(2, stringBottom.position);
    }
}
