using UnityEngine;

[DefaultExecutionOrder(-100)]
public class BowMode : MonoBehaviour
{
    [SerializeField] private Transform leftControllerTransform;
    [SerializeField] private float dashTiltThreshold = -60f;
    [SerializeField] private ShootingMode shootingMode;
    [SerializeField] private BowModelHandler bowModelHandler;

    [SerializeField] private PlayerState state = PlayerState.Shooting;
    public PlayerState State => state;
    public Transform DashController => leftControllerTransform;

    private void OnEnable()
    {
        state = PlayerState.Shooting;
        ApplyState();
    }

    private void Update()
    {
        PlayerState newState = PlayerState.Shooting;
        if (leftControllerTransform != null &&
            Mathf.DeltaAngle(0f, leftControllerTransform.localEulerAngles.z) <= dashTiltThreshold)
        {
            newState = PlayerState.Dashing;
            Debug.Log($"Entered dashing state + {leftControllerTransform.localEulerAngles.z}");
        }

        if (State != newState)
        {
            state = newState;
            ApplyState();
        }
    }

    private void ApplyState()
    {
        if (shootingMode != null) shootingMode.canShoot = State == PlayerState.Shooting;
    }
}
