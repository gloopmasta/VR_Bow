using UnityEngine;

public class TrailSegment : MonoBehaviour
{
    public float lifetime = 7f;
    public string spawnedByTag; 

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
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
