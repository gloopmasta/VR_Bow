using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;
using Cysharp.Threading.Tasks;

public class Player : MonoBehaviour, IDamageable
{
    [Header("ScriptableObjects")]
    [SerializeField] private PlayerDataSO data;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;
    [SerializeField] private LevelEventsSO levelEvents;


    [Header("Player Variables")]
    [SerializeField] private int hp;
    [SerializeField] private int arrowCount;
    //[SerializeField] private float fuel;
    [SerializeField] private float slowTime;
    [SerializeField] private PlayerState state;
    public bool isBashing;
    public int score;

    [Header("Respawn")]
    public Vector3 respawnPosition = Vector3.zero;
    public Vector3 respawnRotation = Vector3.zero;



    public int Hp
    {
        get { return hp; }
        set { hp = Mathf.Min(value, data.MaxHp); } //Make sure that health never exceeds max Health
    }
    public int ArrowCount
    {
        get { return arrowCount; }
        set { arrowCount = Mathf.Clamp(value, 0, data.MaxArrowCount); }
    }
    [PandaVariable]
    public float SlowTime
    {
        get { return slowTime; }
        set { slowTime = value; }
    }
    //public float Fuel
    //{
    //    get { return fuel; }
    //    set { fuel = Mathf.Clamp(value, 0, data.MaxFuel); }
    //}
    public PlayerState State
    {
        get { return state; }
        set { state = value; }
    }
    public bool IsBashing
    {
        get { return isBashing; }
        set { isBashing = value; }
    }
    public int Score
    {
        get { return score; }
        set { score = value; }
    }


    void Start()
    {
        hp = data.MaxHp;
        arrowCount = 6/*data.maxArrowCount*/;
        slowTime = 4f;
        //fuel = data.MaxFuel;

    }

    private void OnEnable()
    {
        arrowHitEvents.OnArrowHitEnemy += IncreaseScore;
        GameManager.Instance.SetPlayer(gameObject);
    }

    private void OnDisable()
    {
        GameManager.Instance.RemovePlayer();
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        //playerEvents.RaiseDamage(amount);

        if (hp <= 0)
        {
            levelEvents.RaiseLevelOneLose(); //lose first level
        }
    }

    public async UniTaskVoid Respawn()
    {
        Debug.Log("Player respawned");
        var ui = GetComponent<PlayerUIManager>();

        await ui.FadeToBlackAsync(); //wait fade to black

        if (respawnPosition != null)
        { 
            transform.position = respawnPosition;   //reset position
            transform.rotation = Quaternion.Euler(respawnRotation);
        }
        else
            Debug.Log("respawnPosition is null");

        GetComponent<Rigidbody>().velocity = Vector3.zero; //Reset velocity
        GetComponent<Rigidbody>().rotation = Quaternion.Euler(Vector3.zero); //Reset velocity
        transform.rotation = Quaternion.Euler(Vector3.zero);
        

        await ui.FadeFromBlackAsync(); //fade out again


    }

    public void IncreaseScore(int ammount)
    {
        score += ammount;
        //playerEvents.RaiseScoreChanged(score);

        
    }

    void OnTriggerEnter(Collider other)
    {
        //on collision with a powerup -> collect logic
        if (other.TryGetComponent<Powerup>(out var powerup))
        {
            powerup.Collect(gameObject);
        }

        //if (other.TryGetComponent<JumpPad>(out var jp))
        //{
        //    jp.Activate(gameObject);
        //}
        //if (other.TryGetComponent<Launchable>(out var launchable) && isBashing) //if player interacts with a launchable and is bashing
        //{
        //    launchable.OnBash(gameObject);
        //}
    }

    void DisableOffRoad()
    {
        GetComponent<OffRoadTracker>().enabled = false;
    }
    void EnableOffRoad()
    {
        GetComponent<OffRoadTracker>().enabled = true;
    }
}



