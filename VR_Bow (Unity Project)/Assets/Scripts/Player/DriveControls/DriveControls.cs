using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DriveControls : MonoBehaviour
{


    [Header("Speed Settings")]
    [SerializeField] float currentSpeed = 5f;
    [SerializeField] float maxSpeed = 100f;
    [SerializeField] float turnSpeed;
    [SerializeField] float driftSpeed;
    [SerializeField] float rotOffset = 90f;

    [Header("Bash Settings")]
    [SerializeField] float bashStrength = 5f;
    [SerializeField] int bashDamage = 1;

    [Header("Jump Settings")]
    [SerializeField] float jumpStrength = 5f;


    [Header("Scripts")]
    [SerializeField] XRControllerData controllerData;

    //How much the handle is turned
    [SerializeField] float revStrength = 1f;

    [Range(1.002f, 1.01f)]
    [SerializeField] float accelerationRate = 1.005f;


    private void Update()
    {
        UpdateControllerData();
        Drive();
        Steer();

    }

    private void UpdateControllerData()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        {
            if (leftVelocity.z >= 2.5f) //swing forward
            { 
                Bash();
                TriggerHapticFeedback();
            }
            if (leftVelocity.y >= 2.5f)
            {
                Jump();
                TriggerHapticFeedback();
            }
        }
    }

    void Bash()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * bashStrength, ForceMode.Impulse);
        TriggerHapticFeedback();
        Debug.Log("Bashed");
    }

    void Jump()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(transform.up * jumpStrength, ForceMode.Impulse);
    }
    void Drive()
    {
        // Only accelerate if below max speed
        if (currentSpeed < maxSpeed)
        {
            currentSpeed *= accelerationRate;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // Clamp to maxSpeed
        }

        // Move forward
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        //TODO: decellerate
    }

    private float currentSteeringInput = 0f;
    public float steeringSmoothness = 5f; // Adjust for desired smoothness

    void Steer()
    {
        if (controllerData._leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
        {
            
            Vector3 eulerRotation = deviceRotation.eulerAngles;
            float targetYaw = eulerRotation.y + rotOffset;
            float targetSteeringInput = Mathf.DeltaAngle(0f, targetYaw);

            // Smoothly interpolate between current and target steering input
            currentSteeringInput = Mathf.Lerp(currentSteeringInput, targetSteeringInput, Time.deltaTime * steeringSmoothness);

            transform.Rotate(Vector3.up, currentSteeringInput * turnSpeed * Time.deltaTime);
        }
    }


    public void TriggerHapticFeedback()
    {
        // Get the right controller
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (leftController.isValid)
        {
            // Send haptic impulse
            leftController.SendHapticImpulse(0, 1f, 0.5f);
        }
    }

}
