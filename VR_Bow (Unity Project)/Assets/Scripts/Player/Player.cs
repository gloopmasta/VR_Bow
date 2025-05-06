using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerDataSO data;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;



    private int hp;
    private int arrowCount;
    private float fuel;
    private float slowTime;
    private PlayerState state;
    private bool isBashing;
    private int score;

    

    
    public int Hp
    {
        get { return hp; }
        set { hp = Mathf.Min(value, data.maxHp); } //Make sure that health never exceeds max Health
    }
    public int ArrowCount
    {
        get { return arrowCount; }
        set { arrowCount = Mathf.Clamp(value, 0, data.maxArrowCount); }
    }
    [PandaVariable] public float SlowTime
    {
        get { return slowTime; }
        set { slowTime = value; }
    }
    public float Fuel
    {
        get { return fuel; }
        set { fuel = Mathf.Clamp(value, 0, data.maxFuel); }
    }
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
        hp = data.maxHp;
        arrowCount = data.maxArrowCount;
        slowTime = 4f;
        fuel = data.maxFuel;
        
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

    public void ConsumeFuel(float amount)
    {
        fuel = Mathf.Clamp(fuel - amount, 0, data.maxFuel);
        //playerEvents.RaiseFuelChanged(fuel);
    }

    public void RefillFuel()
    {
        fuel = data.maxFuel;
        //playerEvents.RaiseFuelChanged(fuel);
    }

    public void IncreaseScore(int ammount)
    {
        score += ammount;
        //playerEvents.RaiseScoreChanged(score);
    }
}

