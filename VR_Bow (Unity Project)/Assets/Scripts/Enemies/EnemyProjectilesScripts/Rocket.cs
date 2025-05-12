using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float rocketSpeed = 30f;
    [SerializeField] private bool usePrediction = false;

    private Transform player;

    private void Start()
    {
        // Find the player object by tag
        player = GameObject.FindWithTag("Player")?.transform;
        if (usePrediction && player == null)
        {
            Debug.LogWarning("Rocket: No player found with tag 'Player'.");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        Vector3 direction;

        if (usePrediction && player != null)
        {
            DriveControls driveControls = player.GetComponent<DriveControls>();
            if (driveControls != null)
            {
                Vector3 playerVelocity = player.forward * driveControls.currentSpeed;

                // Calculate how long it would take the rocket to reach the player
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                float predictionTime = distanceToPlayer / rocketSpeed;

                // Predict future position
                Vector3 futurePosition = player.position + playerVelocity * predictionTime;

                // Calculate direction to that future position
                direction = (futurePosition - transform.position).normalized;
            }
            else
            {
                Debug.LogWarning("Rocket: Player does not have DriveControls component.");
                direction = GetRandomDirection();
            }
        }
        else
        {
            direction = GetRandomDirection();
        }

        // Set velocity directly for more accurate movement
        rb.velocity = direction * rocketSpeed;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(1);
            Destroy(gameObject);
        }
    }

    private Vector3 GetRandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
    }
}
