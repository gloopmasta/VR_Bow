using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rider : Enemy
{
    public SwitchTimeEventsSO switchEvents;
    public ArrowHitEventsSO arrowHitEvents;

    [SerializeField] private int maxHP = 3;


    private void Start()
    {
        Hp = maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<Player>().IsBashing)
            {
                TakeDamage(1);
                GetComponent<Rigidbody>().AddForce(transform.up * 15, ForceMode.Impulse);
                switchEvents.RaiseEnterDSSwitchTime();
            }
        }

        if (other.CompareTag("Arrow"))
        {
            TakeDamage(1);
        }
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }
}
