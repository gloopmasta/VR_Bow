using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float lifetime = 5f;
    private Vector3 velocity;
    private bool useGravity;
    private float timer;

    // Rotation correction if needed (e.g., to align model)
    private static readonly Quaternion rotationCorrection = Quaternion.Euler(-90f, 0f, 0f);

    public void Launch(Vector3 direction, float force, float flexRatio)
    {
        velocity = direction.normalized * force;
        useGravity = flexRatio < 0.9f;
        transform.rotation = Quaternion.LookRotation(velocity) * rotationCorrection;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // Apply gravity manually if needed
        if (useGravity)
        {
            velocity += Physics.gravity * dt;
        }

        // Move manually
        transform.position += velocity * dt;

        // Smooth rotation
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity) * rotationCorrection;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dt * 10f);
        }

        // Despawn after lifetime
        timer += dt;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);
        Destroy(gameObject); // You can replace this with object pooling
    }
}
