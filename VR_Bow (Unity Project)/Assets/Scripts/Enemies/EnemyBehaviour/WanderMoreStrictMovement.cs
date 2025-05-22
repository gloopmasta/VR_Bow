using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderMoreStrictMovement : MonoBehaviour, ITimeScalable
{
    private enum WanderState { DrivingStraight, Turning }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 180f;

    [Header("Timing Settings")]
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float turnAngleRange = 90f;

    private WanderState currentState = WanderState.DrivingStraight;
    private float stateTimer;
    private Quaternion targetRotation;

    private float currentTimeScale = 1f;
    private float baseMoveDuration;
    private float scaledMoveDuration;

    private void Start()
    {
        baseMoveDuration = moveDuration;
        scaledMoveDuration = baseMoveDuration;

        GameManager.Instance.Register(this);
        EnterDrivingState();
    }

    private void Update()
    {
        // Always move forward like a car
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        switch (currentState)
        {
            case WanderState.DrivingStraight:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    EnterTurningState();
                }
                break;

            case WanderState.Turning:
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

                // If the target rotation is reached (or nearly), switch back to driving
                if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
                {
                    EnterDrivingState();
                }
                break;
        }
    }

    private void EnterDrivingState()
    {
        currentState = WanderState.DrivingStraight;
        stateTimer = scaledMoveDuration;
    }

    private void EnterTurningState()
    {
        currentState = WanderState.Turning;

        float randomAngle = Random.Range(-turnAngleRange, turnAngleRange);
        targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + randomAngle, 0f);
    }

    public void OnTimeScaleChanged(float newScale)
    {
        currentTimeScale = newScale;
        scaledMoveDuration = baseMoveDuration / newScale;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Unregister(this);
    }
}
