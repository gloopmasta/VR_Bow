using UnityEngine;

[DefaultExecutionOrder(-100)]
public class BowMode : MonoBehaviour
{
    [SerializeField] private Transform leftControllerTransform;
    [SerializeField] private float thresholdWhileShooting = -60f;
    [SerializeField] private float thresholdWhileDashing = -15f;
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
        PlayerState? newState = null;

        // different threshholds based on wether dashing or shooting
        switch (state)
        {
            case PlayerState.Dashing:
                if (leftControllerTransform != null && Mathf.DeltaAngle(0f, leftControllerTransform.localEulerAngles.z) > thresholdWhileDashing)
                {
                    newState = PlayerState.Shooting;
                }
                break;

            case PlayerState.Shooting:
                if (leftControllerTransform != null && Mathf.DeltaAngle(0f, leftControllerTransform.localEulerAngles.z) <= thresholdWhileShooting)
                {
                    newState = PlayerState.Dashing;
                }
                break;
        }

        if (State != newState && newState != null)
        {
            state = newState ?? PlayerState.Dashing; //apply new state if different
            ApplyState();
        }
    }

    private void ApplyState()
    {
        if (shootingMode != null) shootingMode.canShoot = State == PlayerState.Shooting;
    }
}
