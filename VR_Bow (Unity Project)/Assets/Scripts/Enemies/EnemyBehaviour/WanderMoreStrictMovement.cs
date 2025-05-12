using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderMoreStrictMovement : MonoBehaviour
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

    private void Start()
    {
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

                // Als de rotatie (bijna) bereikt is, terug naar rijden
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
        stateTimer = moveDuration;
    }

    private void EnterTurningState()
    {
        currentState = WanderState.Turning;

        float randomAngle = Random.Range(-turnAngleRange, turnAngleRange);
        targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + randomAngle, 0f);
    }
}
