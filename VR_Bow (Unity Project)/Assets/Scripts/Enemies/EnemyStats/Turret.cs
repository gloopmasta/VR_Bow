using UnityEngine;
using PandaBT;

public class Turret : Enemy
{
    [Header("Turret Settings")]
    [SerializeField] private int maxHP = 1;

    [Header("Event Channels")]
    [SerializeField] private SwitchTimeEventsSO switchEvents;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;

    [Header("Particles")]
    [SerializeField] private ParticleEffectsLibrary effectsLibrary;

    [Header("Sound effects")]
    [SerializeField] private AudioCue explosionSoundCue;

    private void Start()
    {
        Hp = maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        // React to Player bash
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.IsBashing)
            {
                TakeDamage(1);
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(transform.up * 15f, ForceMode.Impulse);
                }

                switchEvents?.RaiseEnterDSSwitchTime();
            }
        }

        // React to arrow hit
        if (other.CompareTag("Arrow"))
        {
            TakeDamage(1);
        }
    }

    protected override void Die()
    {
        //particles
        effectsLibrary.PlayParticle(transform.position, effectsLibrary.turretExplode);

        

        //score
        arrowHitEvents?.RaiseScoreHitEnemy(100);

        // Gebruik AudioCue om geluid te spelen op het midden van elke laser
        if (SoundEffectManager.Instance != null && explosionSoundCue != null)
        {
            SoundEffectManager.Instance.Play3D(explosionSoundCue, transform.position);
        }

        Destroy(gameObject);
    }
}

