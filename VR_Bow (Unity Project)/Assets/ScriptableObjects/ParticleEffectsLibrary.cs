using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Libraries/Particle Library")]
public class ParticleEffectsLibrary : ScriptableObject
{
    [Header("Enemies")]
    public GameObject arrow;

    [Header("Enemies")]
    public GameObject turretExplode;

    public void PlayParticle(Vector3 position, GameObject particlePrefab)
    {
        var effect = Instantiate(particlePrefab, position, Quaternion.identity);
        Destroy(effect, 2f);
    }
}
