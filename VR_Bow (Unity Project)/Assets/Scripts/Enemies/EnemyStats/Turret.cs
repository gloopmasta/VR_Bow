using UnityEngine;
using PandaBT;

public class Turret : Enemy
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

        if (other.CompareTag("Projectile"))
        {
            //als hij word gehit door een arrow
        }
    }

    protected override void Die()
    {
        // als enemy doodgaan
    }
}
