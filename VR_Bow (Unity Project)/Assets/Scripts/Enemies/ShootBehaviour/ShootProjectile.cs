using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ShootProjectile : MonoBehaviour, ITimeScalable
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootingInterval = 1f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private bool turnHead = false;
    [SerializeField] private GameObject headToTurn;
    [SerializeField] private float headTurnSpeed = 5f;


    private float baseShootingInterval;
    private float scaledShootingInterval = 1f;

    private bool playerInRange = false;
    private float lastShootTime;
    private bool isLaserActive = false;

    private Transform player;
    private bool lastShotUsedPrediction = false;

    private void Start()
    {
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = detectionRange;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        baseShootingInterval = shootingInterval;
        scaledShootingInterval = baseShootingInterval;

        GameManager.Instance.Register(this);

    }


    private void Update()
    {
        if (!playerInRange || isLaserActive) return;

        // Smoothly rotate head toward player if using prediction mode
        if (turnHead && headToTurn != null && player != null && lastShotUsedPrediction)
        {
            Vector3 directionToPlayer = (player.position - headToTurn.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            headToTurn.transform.rotation = Quaternion.Slerp(headToTurn.transform.rotation, targetRotation, Time.deltaTime * headTurnSpeed);
        }

        if (Time.time - lastShootTime >= scaledShootingInterval)
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

        GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        if (projectileInstance.TryGetComponent<Rocket>(out var rocket))
        {
            rocket.Initialize();
            lastShotUsedPrediction = rocket.usePrediction;

            if (turnHead && headToTurn != null && !rocket.usePrediction)
            {
                Vector3 lookDir = rocket.LaunchDirection;
                if (lookDir != Vector3.zero)
                {
                    headToTurn.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                }
            }
        }

        if (projectileInstance.TryGetComponent<LaserBeam>(out var laser))
        {
            if (turnHead && headToTurn != null)
            {
                laser.headToTurn = headToTurn.transform;
            }

            isLaserActive = true;
            laser.onLaserComplete = () => isLaserActive = false;
        }
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

    public void OnTimeScaleChanged(float newScale)
    {
        scaledShootingInterval = baseShootingInterval / newScale;
    }

    private void OnDestroy()
    {
        GameManager.Instance.Unregister(this);
    }

}
