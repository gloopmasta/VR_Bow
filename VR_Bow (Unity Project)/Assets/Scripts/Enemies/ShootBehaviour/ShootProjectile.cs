using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float shootingInterval = 1f;

    private float lastShootTime;

    private void Update()
    {
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
}
