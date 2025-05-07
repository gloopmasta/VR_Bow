using UnityEngine;

public class WanderMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 120f;
    public float obstacleCheckDistance = 4f;
    public float edgeCheckDistance = 3f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private float turnCooldown = 0f;

    void Update()
    {
        // Rijd vooruit
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Check cooldown zodat hij niet te vaak draait
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
            turnCooldown = 1f; // voorkomt dat hij elke frame draait
        }
    }

    void TurnRandom()
    {
        float randomAngle = Random.Range(-110f, 110f);
        transform.Rotate(Vector3.up, randomAngle);
    }

    // Debug visualization in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + transform.forward * edgeCheckDistance, Vector3.down * 5f);
    }
}
