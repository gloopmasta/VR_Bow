using UnityEngine;

public class WanderMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;                // Speed at which the object moves forward
    [SerializeField] private float rotationSpeed = 120f;          // Not currently used, but can be applied for smooth turning

    [Header("Detection Settings")]
    [SerializeField] private float obstacleCheckDistance = 4f;    // Distance to check for obstacles in front
    [SerializeField] private float edgeCheckDistance = 3f;        // Distance ahead to check for edges
    [SerializeField] private LayerMask groundLayer;               // LayerMask for what is considered ground
    [SerializeField] private LayerMask obstacleLayer;             // LayerMask for what is considered an obstacle

    private float turnCooldown = 0f;                              // Timer to prevent constant turning

    private void Update()
    {
        // Move forward constantly
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Countdown cooldown to limit turning frequency
        if (turnCooldown > 0f)
        {
            turnCooldown -= Time.deltaTime;
            return;
        }

        bool obstacleAhead = Physics.Raycast(transform.position, transform.forward, obstacleCheckDistance, obstacleLayer);
        bool edgeAhead = !Physics.Raycast(transform.position + transform.forward * edgeCheckDistance, Vector3.down, 5f, groundLayer);

        if (obstacleAhead || edgeAhead)
        {
            TurnRandom();
            turnCooldown = 1f; // Prevents immediate retrigger
        }
    }

    private void TurnRandom()
    {
        float randomAngle = Random.Range(-110f, 110f);
        transform.Rotate(Vector3.up, randomAngle);
    }

    // Debug rays in the scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + transform.forward * edgeCheckDistance, Vector3.down * 5f);
    }
}
