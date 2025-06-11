using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SlopeWindAssist : MonoBehaviour
{
    [SerializeField] private float windForce = 5f;
    [SerializeField] private Vector3 windDirection = Vector3.up; // Adjust to match slope angle

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            // Apply force in the slope's upward direction
            other.attachedRigidbody.AddForce(windDirection.normalized * windForce, ForceMode.Acceleration);
        }
    }

    // Visualize wind direction in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + windDirection.normalized * 2f);
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
    }
}