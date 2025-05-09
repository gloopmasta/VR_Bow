using UnityEngine;

public class TrailSpawner : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] GameObject trailSegmentPrefab;    // Prefab for individual trail segments
    [SerializeField] private float segmentSpacing = 0.01f;     // Minimum distance before spawning a new segment

    private Vector3 lastPosition;
    private bool isSpawning = false;
    private Transform trailParent;                             // Parent object to organize spawned segments

    private void OnEnable()
    {
        // Try to load the prefab from Resources if not assigned manually
        if (trailSegmentPrefab == null)
        {
            trailSegmentPrefab = Resources.Load<GameObject>("TrailSegment");

            if (trailSegmentPrefab == null)
            {
                Debug.LogError("TrailSpawner: No trailSegmentPrefab assigned or found in Resources folder.");
                enabled = false;
                return;
            }
        }

        isSpawning = true;
        lastPosition = transform.position;

        // Create a parent object for trail segments to keep the hierarchy clean
        GameObject parentObject = new GameObject($"{gameObject.name}_Trail");
        trailParent = parentObject.transform;
    }

    private void Update()
    {
        if (!isSpawning || trailSegmentPrefab == null) return;

        float distance = Vector3.Distance(transform.position, lastPosition);
        if (distance >= segmentSpacing)
        {
            SpawnSegment(transform.position);
            lastPosition = transform.position;
        }
    }

    private void SpawnSegment(Vector3 currentPosition)
    {
        Vector3 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;
        if (distance == 0f) return;

        Vector3 midPoint = lastPosition + (direction / 2);

        GameObject segment = Instantiate(
            trailSegmentPrefab,
            midPoint,
            Quaternion.LookRotation(direction),
            trailParent
        );

        // Adjust the scale based on the distance between segments
        segment.transform.localScale = new Vector3(
            segment.transform.localScale.x,
            segment.transform.localScale.y,
            distance
        );

        // Assign the tag of the spawner to the segment for damage logic
        TrailSegment trail = segment.GetComponent<TrailSegment>();
        if (trail != null)
        {
            trail.spawnedByTag = gameObject.tag;
        }
    }
}
