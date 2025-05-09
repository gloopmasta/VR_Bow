using UnityEngine;

public class TrailSegment : MonoBehaviour
{
    [SerializeField] private float lifetime = 7f;           // How long the segment exists before disappearing
    public string spawnedByTag;           // Tag of the object that spawned this segment ("Player" or "Enemy")

    private void Start()
    {
        // Automatically destroy this segment after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the segment hits a valid target and apply damage
        if (other.CompareTag("Player") && spawnedByTag == "Enemy")
        {
            other.GetComponent<IDamageable>()?.TakeDamage(1);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Enemy") && spawnedByTag == "Player")
        {
            other.GetComponent<IDamageable>()?.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}
