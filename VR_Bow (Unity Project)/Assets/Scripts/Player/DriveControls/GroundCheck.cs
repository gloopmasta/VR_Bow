using UnityEngine;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour
{
    private HashSet<Collider> groundColliders = new HashSet<Collider>();
    [SerializeField] private JumpEventsSO jumpEvents;

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            groundColliders.Add(other);
            jumpEvents.RaiseLand();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            groundColliders.Remove(other);
            jumpEvents.RaiseJump();
        }
    }

    public bool IsGrounded()
    {
        return groundColliders.Count > 0;
    }
}
