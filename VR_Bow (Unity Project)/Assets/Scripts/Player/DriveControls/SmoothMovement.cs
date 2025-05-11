using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SmoothMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector2 inputDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        inputDirection = new Vector2(0f, 1f);
    }

    void FixedUpdate()
    {
        // Convert input to 3D movement (adjust for VR forward direction)
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (forward * inputDirection.y + right * inputDirection.x).normalized;

        // Calculate movement using unscaled physics time
        float unscaledDelta = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
        Vector3 velocityChange = movement * moveSpeed * unscaledDelta;

        // Apply movement while preserving vertical velocity (gravity)
        Vector3 newVelocity = rb.velocity;
        newVelocity.x = velocityChange.x;
        newVelocity.z = velocityChange.z;
        rb.velocity = newVelocity;

        Debug.Log(Time.timeScale);
    }
}