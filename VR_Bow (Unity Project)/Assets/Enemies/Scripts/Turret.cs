using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class Turret : Enemy
{
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
}
