using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float rocketSpeed = 30f;
    public bool usePrediction = false;

    private Transform player;
    private Rigidbody rb;
    public Vector3 LaunchDirection { get; private set; }

    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        Vector3 direction;

        player = GameObject.FindWithTag("Player")?.transform;

        if (usePrediction && player != null)
        {
            DriveControls driveControls = player.GetComponent<DriveControls>();
            if (driveControls != null)
            {
                Vector3 playerVelocity = player.forward * driveControls.currentSpeed;
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                float predictionTime = distanceToPlayer / rocketSpeed;
                Vector3 futurePosition = player.position + playerVelocity * predictionTime;
                direction = (futurePosition - transform.position).normalized;
            }
            else
            {
                direction = GetRandomDirection();
            }
        }
        else
        {
            direction = GetRandomDirection();
        }

        LaunchDirection = direction;

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
