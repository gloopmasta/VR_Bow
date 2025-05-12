using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;

public class Player : MonoBehaviour, IDamageable
{
    [Header("ScriptableObjects")]
    [SerializeField] private PlayerDataSO data;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;
    


    [Header("Player Variables")]
    [SerializeField] private int hp;
    [SerializeField] private int arrowCount;
    //[SerializeField] private float fuel;
    [SerializeField] private float slowTime;
    [SerializeField] private PlayerState state;
    public bool isBashing;
    [SerializeField] private int score;



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
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        //playerEvents.RaiseDamage(amount);

        if (hp <= 0)
        {
            //playerEvents.RaiseDeath();
            // e.g., Disable player, trigger animation, etc.
        }
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
}



