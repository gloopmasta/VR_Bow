using UnityEngine;
using PandaBT;

public class Turret : Enemy
{
    [Header("Turret Settings")]
    [SerializeField] private int maxHP = 1;

    [Header("Event Channels")]
    [SerializeField] private SwitchTimeEventsSO switchEvents;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;

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
        Destroy(gameObject);
        arrowHitEvents?.RaiseScoreHitEnemy(100);
    }
}

