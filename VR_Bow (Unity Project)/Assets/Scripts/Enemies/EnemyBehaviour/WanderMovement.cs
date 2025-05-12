using UnityEngine;

public class WanderMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 360f; // degrees/sec

    [Header("Detection Settings")]
    [SerializeField] private float obstacleCheckDistance = 4f;
    [SerializeField] private float edgeCheckDistance = 3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Quaternion targetRotation;
    private float directionChangeCooldown = 0f;

    private void Start()
    {
        PickNewRandomWorldDirection();
    }

    private void Update()
    {
        // Forward motion
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Smooth rotation towards targetRotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Timer voor koerswijziging
        directionChangeCooldown -= Time.deltaTime;
        if (directionChangeCooldown <= 0f)
        {
            PickNewRandomWorldDirection();
        }

        // Detecteer obstakels en randen
        bool obstacle = Physics.Raycast(transform.position, transform.forward, obstacleCheckDistance, obstacleLayer);
        bool edge = !Physics.Raycast(transform.position + transform.forward * edgeCheckDistance, Vector3.down, 5f, groundLayer);

        if (obstacle || edge)
        {
            PickNewRandomWorldDirection(force: true);
        }
    }

    private void PickNewRandomWorldDirection(bool force = false)
    {
        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // Stel een wereldwijde richting in, onafhankelijk van huidige richting
        if (randomDir.sqrMagnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(randomDir);
        }

        directionChangeCooldown = force ? 1f : Random.Range(2f, 4f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + transform.forward * edgeCheckDistance, Vector3.down * 5f);
    }
}
