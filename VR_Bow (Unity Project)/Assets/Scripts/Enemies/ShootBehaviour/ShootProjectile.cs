using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ShootProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootingInterval = 1f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private bool turnHead = false;
    [SerializeField] private GameObject headToTurn;

    private bool playerInRange = false;
    private float lastShootTime;

    private void Start()
    {
        // Automatically configure the SphereCollider as a trigger with the desired detection range
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = detectionRange;
    }

    private void Update()
    {
        LaserBeam laserBeamScript = projectilePrefab.GetComponent<LaserBeam>();
        Rocket rocketScript = projectilePrefab.GetComponent<Rocket>();

        if (laserBeamScript != null)
        {
           //headToTurn.transform.rotation == script .slerp naar directiobn
        }
        if (rocketScript != null && rocketScript.usePrediction) //prefab is rocket and uses prediction
        {
            //Rotate naar player altijd
        }

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
            Debug.LogWarning("ShootProjectile: No projectile prefab assigned.");
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
