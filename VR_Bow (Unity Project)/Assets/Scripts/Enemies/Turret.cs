using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;
using PandaBT;

public class Turret : Enemy
{
    public SwitchTimeEventsSO switchEvents;
    public ArrowHitEventsSO arrowHitEvents;
    public GameObject projectilePrefab;
    [PandaVariable] public float shootingInterval = 1f;



    private void Awake()
    {
        Hp = 2;
    }

    void Update()
    {
        
    }

    [PandaTask]    
    public void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        //Rigidbody rb = projectile.GetComponent<Rigidbody>();
        //rb.AddForce(transform.forward * 10, ForceMode.Impulse);
        PandaTask.Succeed();
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
        if (other.CompareTag("Projectile"))
        {
                arrowHitEvents.RaiseArrowHitEnemy(100);
        }

    }
}
