using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class Turret : Enemy
{
    public SwitchTimeEventsSO switchEvents;

    private void Awake()
    {
        Hp = 2;
    }

    void Update()
    {
        
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            if (other.GetComponent<Player>().IsBashing) //if player is bashing
            {
                TakeDamage(1); //take damage
                GetComponent<Rigidbody>().AddForce(transform.up * 15, ForceMode.Impulse);  //launch upwards
                switchEvents.RaiseEnterDSSwitchTime(); //Raise enter switch time ds
            }
        }
    }
}
