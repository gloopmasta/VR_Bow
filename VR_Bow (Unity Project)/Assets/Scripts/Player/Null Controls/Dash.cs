using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

public class Dash : MonoBehaviour
{
    [SerializeField] XRControllerData controllerData;
    [SerializeField] Transform leftControllerTransform;
    [SerializeField] GameSettings settings;
    [SerializeField] PlayerDataSO pData;
    [SerializeField] float nextDashTime;
    [SerializeField] float dashStrength = 500f;
    [SerializeField] float dashtiltTreshold = -60f;
    [SerializeField] RumbleManager rumble;

    public bool dashPosssible;

    private Quaternion baseLeftRotation;
    private bool hasBaseLeftRotation;
    private Rigidbody rb;
    private Player player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 50f;

        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        hasBaseLeftRotation = false;
        dashPosssible = false;
    }

    private void Update()
    {
        UpdateDashPossible();
        
        if(!dashPosssible) return;

        UpdateControllerData();
    }

    private void UpdateDashPossible()
    {
        if (leftControllerTransform == null)
        {
            hasBaseLeftRotation = false;
            dashPosssible = false;
            return;
        }

        Quaternion currentRotation = leftControllerTransform.localRotation;

        if (!hasBaseLeftRotation)
        {
            baseLeftRotation = currentRotation;
            hasBaseLeftRotation = true;
        }

        Quaternion relativeRotation = Quaternion.Inverse(baseLeftRotation) * currentRotation;
        float zTilt = Mathf.DeltaAngle(0f, relativeRotation.eulerAngles.z);
        dashPosssible = zTilt <= dashtiltTreshold;
    }

    private void UpdateControllerData()
    {
        InputDevice activeController = controllerData.GetActiveController(settings.rightSteeringController);
        if (activeController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 controllerVelocity))
        {
            float now = Time.time;
            if (dashPosssible && controllerVelocity.z >= 2.5f && now >= nextDashTime)
            {
                StartDash();
                nextDashTime = now + pData.BashCooldown;
            }
        }
    }

    private void StartDash()
    {
        rb.AddForce(transform.forward * dashStrength, ForceMode.Impulse);
        MakeBashActive().Forget();
    }

    private async UniTaskVoid MakeBashActive()
    {
        player.isBashing = true;
        await UniTask.WaitForSeconds(pData.BashActiveTime);
        player.isBashing = false;
    }
}
