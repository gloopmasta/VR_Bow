using UnityEngine;

public class Rider : Enemy
{
    [Header("Rider Settings")]
    [SerializeField] private int maxHP = 3;
    [SerializeField] private SwitchTimeEventsSO switchEvents;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;

    private void Start()
    {
        Hp = maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.IsBashing)
            {
                TakeDamage(1);
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(Vector3.up * 15f, ForceMode.Impulse);
                }

                switchEvents.RaiseEnterDSSwitchTime();
            }
        }
        else if (other.CompareTag("Arrow"))
        {
            TakeDamage(1);
            // You could also raise arrowHitEvents here if needed
        }
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }
}
