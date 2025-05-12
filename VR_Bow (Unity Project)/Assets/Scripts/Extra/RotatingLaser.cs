using UnityEngine;

public class RotatingLaser : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float laserLength = 5f;
    [SerializeField] private string spawnedByTag;

    private LineRenderer lineRenderer;
    private GameObject laserPivot;
    private float nextDamageTime;

    private void Start()
    {
        // Create a separate pivot object that is not affected by parent's rotation
        laserPivot = new GameObject("LaserPivot");
        laserPivot.transform.position = transform.position;
        laserPivot.transform.rotation = Quaternion.identity;

        // Make sure the pivot follows the object positionally but not rotationally
        laserPivot.transform.SetParent(null); // Not parented, world-aligned

        // Add and configure the LineRenderer
        lineRenderer = laserPivot.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    private void Update()
    {
        // Follow the owner’s position
        laserPivot.transform.position = transform.position;

        // Rotate smoothly around world Y axis
        laserPivot.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Determine the laser direction and end point
        Vector3 start = laserPivot.transform.position;
        Vector3 direction = laserPivot.transform.forward;
        Vector3 end = start + direction * laserLength;

        // Set LineRenderer positions
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Damage detection via raycast
        if (Physics.Raycast(start, direction, out RaycastHit hit, laserLength))
        {
            if (Time.time >= nextDamageTime)
            {
                if (spawnedByTag == "Player" && hit.collider.CompareTag("Enemy"))
                {
                    hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
                    nextDamageTime = Time.time + 1f;
                }
                else if (spawnedByTag == "Enemy" && hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
                    nextDamageTime = Time.time + 1f;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (laserPivot != null)
        {
            Destroy(laserPivot);
        }
    }
}
