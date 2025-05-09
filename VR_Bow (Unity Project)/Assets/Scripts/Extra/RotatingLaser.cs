using UnityEngine;

public class RotatingLaser : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;  // Speed at which the laser rotates around its origin
    [SerializeField] private float laserLength = 5f;      // Visual and functional length of the laser beam
    [SerializeField] private string spawnedByTag;         // Expected to be "Player" or "Enemy" to determine damage logic

    private LineRenderer lineRenderer;
    private GameObject laserChild;
    private float nextDamageTime;

    private void Start()
    {
        // Create a child GameObject that visually represents the laser
        laserChild = new GameObject("RotatingLaserChild");
        laserChild.transform.SetParent(transform);
        laserChild.transform.localPosition = Vector3.zero;
        laserChild.transform.localRotation = Quaternion.identity;

        // Add and configure the LineRenderer component
        lineRenderer = laserChild.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        // Ensure the laser always renders, even through objects
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    private void Update()
    {
        // Rotate the laser in world space around the Y axis
        laserChild.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Calculate beam start and end points
        Vector3 laserStart = laserChild.transform.position;
        Vector3 laserDirection = laserChild.transform.forward;
        Vector3 laserEnd = laserStart + laserDirection * laserLength;

        // Update the LineRenderer visuals
        lineRenderer.SetPosition(0, laserStart);
        lineRenderer.SetPosition(1, laserEnd);

        // Check for collisions to apply damage
        if (Physics.Raycast(laserStart, laserDirection, out RaycastHit hit, laserLength))
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
        // Clean up the laser child object when this script is removed
        if (laserChild != null)
        {
            Destroy(laserChild);
        }
    }
}
