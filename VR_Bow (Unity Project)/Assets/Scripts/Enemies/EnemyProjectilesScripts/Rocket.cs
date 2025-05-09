using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private bool usePrediction = false;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        if (usePrediction && player == null)
        {
            Debug.LogWarning("Rocket: Geen speler gevonden met tag 'Player'.");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        Vector3 direction;

        if (usePrediction && player != null)
        {
            DriveControls driveControls = player.GetComponent<DriveControls>();
            Vector3 playerVelocity = player.forward * driveControls.CurrentSpeed;
            float predictionTime = 1.0f;
            Vector3 futurePosition = player.position + playerVelocity * predictionTime;

            direction = (futurePosition - transform.position).normalized;
        }
        else
        {
            direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        }

        rb.AddForce(direction * launchForce, ForceMode.Impulse);
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
}
