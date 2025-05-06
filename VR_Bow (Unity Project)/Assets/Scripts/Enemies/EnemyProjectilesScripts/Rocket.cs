using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rocket : MonoBehaviour
{
    public float launchForce = 10f;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Bepaal willekeurige horizontale richting
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // Pas kracht toe
        rb.AddForce(direction * launchForce, ForceMode.Impulse);

        // Roteer projectile zodat hij in vliegrichting wijst
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        // Verwijder na 10 seconden
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player geraakt!");
            other.GetComponent<IDamageable>().TakeDamage(1);
            Destroy(gameObject); // rocket verdwijnt na impact
        }
    }
}
