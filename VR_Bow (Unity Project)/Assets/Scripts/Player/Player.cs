using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

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
    public bool invulnerable = false;

    [Header("Respawn")]
    public Vector3 respawnPosition = Vector3.zero;
    public Vector3 respawnRotation = Vector3.zero;

    [Header("Invulnerability animation")]
    [SerializeField] Animator vulnerabilityAnimator;

    [Header("Sound Effects")]
    [SerializeField] private AudioCue takeDamageCue;


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
        ResetStats();
    }

    public void ResetStats()
    {
        hp = data.MaxHp;
        arrowCount = data.MaxArrowCount;
        slowTime = 4f;
        score = 0;
        
    }

    private void OnEnable()
    {
        arrowHitEvents.OnArrowHitEnemy += IncreaseScore;
        GameManager.Instance.SetPlayer(gameObject);

        levelEvents.OnLevelOneStart += () => invulnerable = false; // damageable again after level starts
    }

    private void OnDisable()
    {
        GameManager.Instance.RemovePlayer();
    }

    public void TakeDamage(int amount)
    {
        if (invulnerable) 
        {
            return;
        }

        hp -= amount;  //deduct HP

        //Sound effect
        if (SoundEffectManager.Instance != null && takeDamageCue != null)
        {
            SoundEffectManager.Instance.Play(takeDamageCue);
        }

        //animation for indication
        GetComponent<PlayerUIManager>().damageIndicator.SetActive(false);
        GetComponent<PlayerUIManager>().damageIndicator.SetActive(true);

        //invulnerability
        Invulerability().Forget();

        if (hp <= 0)
        {
            levelEvents.RaiseLevelOneLose(); //lose first level
        }
    }

    public async UniTaskVoid Invulerability()
    {
        //vulnerabilityAnimator.enabled = true;
        //vulnerabilityAnimator.Play("start");  play animation

        invulnerable = true;
        await UniTask.WaitForSeconds(data.InvulnerableTime);
        invulnerable = false;

        //vulnerabilityAnimator.enabled = false;//disable animator or end animation
    }

    public async UniTaskVoid Respawn()
    {
        Debug.Log("Player respawned");
        var ui = GetComponent<PlayerUIManager>();
        var rb = GetComponent<Rigidbody>();

        await ui.FadeToBlackAsync(); //wait fade to black

        if (respawnPosition != null)
        { 
            transform.position = respawnPosition;   //reset position
            rb.position = respawnPosition;
            transform.rotation = Quaternion.Euler(respawnRotation);
        }
        else
            Debug.Log("respawnPosition is null");

        rb.velocity = Vector3.zero; //Reset velocity
        rb.rotation = Quaternion.Euler(respawnRotation); //Reset velocity
        rb.angularVelocity = Vector3.zero;


        GetComponent<DriveControls>().currentVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.Euler(respawnRotation));


        await ui.FadeFromBlackAsync(); //fade out again
    }

    public void ResetPosition()
    {
        respawnPosition = Vector3.zero;
        respawnRotation = Vector3.zero;

        var rb = GetComponent<Rigidbody>();

        transform.position = Vector3.zero;   //reset position
        transform.rotation = Quaternion.Euler(Vector3.zero);

        rb.position = Vector3.zero;
        rb.velocity = Vector3.zero; //Reset velocity
        rb.rotation = Quaternion.Euler(respawnRotation); //Reset velocity
        rb.angularVelocity = Vector3.zero;

        GetComponent<DriveControls>().currentVelocity = Vector3.zero;

        rb.MoveRotation(Quaternion.Euler(respawnRotation));
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
        //GetComponent<OffRoadTracker>().enabled = false;
    }
    void EnableOffRoad()
    {
        //GetComponent<OffRoadTracker>().enabled = true;
    }
}



