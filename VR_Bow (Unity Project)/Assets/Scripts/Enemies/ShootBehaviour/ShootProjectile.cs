using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ShootProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    [SerializeField] private float shootingInterval = 1f;
    [SerializeField] private float detectionRange = 25f;

    private bool playerInRange = false;
    private float lastShootTime;

    private void Start()
    {
        // Voeg automatisch een SphereCollider toe als trigger
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = detectionRange;
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Time.time - lastShootTime >= shootingInterval)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned.");
            return;
        }

        Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
