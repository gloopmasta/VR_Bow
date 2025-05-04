using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float lauchStrength = 400f;
    public SwitchTimeEventsSO switchEvents;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Rigidbody>().AddForce(transform.up * lauchStrength, ForceMode.Impulse);
            switchEvents.RaiseEnterDSSwitchTime();
        }
    }
}
