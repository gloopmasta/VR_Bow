using UnityEngine;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour
{
    private HashSet<Collider> groundColliders = new HashSet<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            groundColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            groundColliders.Remove(other);
        }
    }

    public bool IsGrounded()
    {
        return groundColliders.Count > 0;
    }
}
