using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Launchable : MonoBehaviour
{
    [SerializeField] private float launchForce = 100f;
    public BashEventSO bashEvent;
    public SwitchTimeEventsSO switchEvent;
    [SerializeField] private Rigidbody rb;


   
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnBash(collision.gameObject);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //lock rotation on x and z and Y
    }

    public void OnBash(GameObject player)
    {
        if (!player.GetComponent<Player>().isBashing) //if player is not bashing 
        {
            return;
        }
        rb.AddForce(player.transform.forward * launchForce, ForceMode.Impulse);
        rb.AddForce(transform.up * launchForce, ForceMode.Impulse);
        bashEvent.RaiseLaunchingBash();
        switchEvent.RaiseEnterDSSwitchTime();
        Debug.Log("Player bashed into me");
    }
}
