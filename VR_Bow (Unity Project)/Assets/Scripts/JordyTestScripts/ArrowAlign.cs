using UnityEngine;

public class Arrow : MonoBehaviour
{
    private float speed;
    private Rigidbody rb;

    // Rotation correction to align the model properly (adjust if needed)
    private static readonly Quaternion rotationCorrection = Quaternion.Euler(0f, -90f, 90f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Called by Bow when the arrow is fired
    public void Launch(Vector3 direction, float force, float flexRatio)
    {
        speed = force;
        rb.velocity = direction * speed;
        rb.useGravity = flexRatio < 0.9f;

        // Set initial rotation corrected
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity) * rotationCorrection;
        }
    }

    void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity) * rotationCorrection;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);
        Despawn();
    }

    public void Despawn()
    {
        Destroy(gameObject); // Replace with object pooling if needed
    }
}
