using UnityEngine;
using UnityEngine.InputSystem;

public class BowSimulator : MonoBehaviour
{
    [Header("Arrow & Shooting")]
    public GameObject projectilePrefab;
    public Transform shootPoint; // Position only
    public float maxShootForce = 20f;
    public InputActionAsset inputActions;

    [Header("Hand Transforms")]
    public Transform leftHand;   // Bow hand (used for aiming)
    public Transform rightHand;  // Pulling hand (used for drawing)

    [Header("Draw Settings")]
    public float maxDrawDistance = 0.5f;

    [Header("Trajectory Preview")]
    public LineRenderer trajectoryLine;
    public int trajectorySteps = 30;
    public float timeStep = 0.1f;

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

            // Use left hand's forward direction for aiming
            Vector3 launchDir = leftHand.forward;
            float previewForce = flexValue * maxShootForce;
            Vector3 launchVel = launchDir * previewForce;

            DrawTrajectory(shootPoint.position, launchVel);

            Debug.DrawRay(shootPoint.position, launchVel.normalized * 0.5f, Color.red);
        }
        else
        {
            // Hide the trajectory line when not drawing
            if (trajectoryLine != null && trajectoryLine.enabled)
                trajectoryLine.enabled = false;
        }
    }




    void StartDrawing()
    {
        isDrawing = true;
        drawStartPosition = rightHand.position;

        if (trajectoryLine != null)
            trajectoryLine.enabled = true;

        Debug.Log("Started drawing bow.");
    }

    void ReleaseArrow()
    {
        if (!isDrawing) return;

        Shoot();
        isDrawing = false;
        flexValue = 0f;
        Debug.Log("Arrow released.");
    }

    void Shoot()
    {
        float shootForce = flexValue * maxShootForce;
        Vector3 shootDirection = leftHand.forward;
        Debug.Log("Arrow launched with force: " + shootForce);

        GameObject arrowObj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.Launch(shootDirection, shootForce, flexValue);
           
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
            Vector3 point;

            if (ignoreGravity)
            {
                // Straight line (no gravity)
                point = startPos + startVel * t;
            }
            else
            {
                // Parabolic arc with gravity
                point = startPos + startVel * t + 0.5f * Physics.gravity * (t * t);
            }

            trajectoryLine.SetPosition(i, point);
        }
    }

}
