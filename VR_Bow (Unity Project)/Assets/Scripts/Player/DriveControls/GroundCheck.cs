using UnityEngine;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour
{
    private HashSet<Collider> groundColliders = new HashSet<Collider>();
    [SerializeField] private JumpEventsSO jumpEvents;
    [SerializeField] private SwitchTimeEventsSO switchTimeEvents;

    [Header("Slope Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    private Vector3 averageNormal;
    private bool isOnSlope;

    private void FixedUpdate()
    {
        CalculateSlopeNormal();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Slope"))
        {
            groundColliders.Add(other);
            jumpEvents.RaiseLand();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (groundColliders.Contains(other))
        {
            groundColliders.Remove(other);
            //jumpEvents.RaiseJump();
        }
    }

    //only return true if no ground colliders found
    public bool IsGrounded() => groundColliders.Count > 0;

    public bool IsOnSlope() => isOnSlope;

    public Vector3 GetSlopeMoveDirection(Vector3 desiredDirection)
    {
        if (!isOnSlope) return desiredDirection;
        return Vector3.ProjectOnPlane(desiredDirection, averageNormal).normalized;
    }

    private void CalculateSlopeNormal()
    {
        if (!IsGrounded())
        {
            isOnSlope = false;
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            raycastDistance,
            groundLayer
        );

        if (hits.Length == 0)
        {
            isOnSlope = false;
            return;
        }

        // Calculate average normal
        Vector3 normalSum = Vector3.zero;
        foreach (var hit in hits) normalSum += hit.normal;
        averageNormal = (normalSum / hits.Length).normalized;

        float slopeAngle = Vector3.Angle(averageNormal, Vector3.up);
        isOnSlope = slopeAngle > 5f && slopeAngle <= maxSlopeAngle;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (isOnSlope)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, averageNormal * 2f);
            Gizmos.DrawRay(transform.position, GetSlopeMoveDirection(transform.forward) * 2f);
        }
    }
}