using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Particle Event")]
public class ParticleEventSO : ScriptableObject
{
    public Action<Vector3, GameObject> OnPlayParticle; // Position + Prefab

    public void Raise(Vector3 position, GameObject particlePrefab)
    {
        OnPlayParticle?.Invoke(position, particlePrefab);
    }
}
