using UnityEngine;

public class Rocket : MonoBehaviour, ITimeScalable
{
    [SerializeField] private float rocketSpeed = 30f;
    public bool usePrediction = false;

    private Transform player;
    private Rigidbody rb;
    public Vector3 LaunchDirection { get; private set; }

    private float currentSpeedMultiplier = 1f;

    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Haal de huidige timescale op van de GameManager
        if (GameManager.Instance != null && GameManager.Instance.IsInSlowTime())
        {
            currentSpeedMultiplier = GameManager.Instance.CurrentTimeScale;
        }

        player = GameObject.FindWithTag("Player")?.transform;

        Vector3 direction;
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
        rb.velocity = direction * rocketSpeed * currentSpeedMultiplier;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        // Registreer bij GameManager om tijdschaling te ondersteunen
        GameManager.Instance?.Register(this);

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

    public void OnTimeScaleChanged(float scale)
    {
        currentSpeedMultiplier = scale;
        if (rb != null)
        {
            rb.velocity = LaunchDirection * rocketSpeed * currentSpeedMultiplier;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance?.Unregister(this);
    }
}
