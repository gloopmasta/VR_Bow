using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerDataSO data;
    


    [SerializeField] private int hp;
    [SerializeField] private int arrowCount;
    [SerializeField] private float fuel;
    [SerializeField] private float slowTime;
    [SerializeField] private PlayerState state;
    [SerializeField] private bool isBashing;

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


    void Start()
    {
        hp = data.maxHp;
        arrowCount = 1/*data.maxArrowCount*/;
        slowTime = 4f;
        fuel = data.maxFuel;
        
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

    private void OnTriggerEnter(Collider other)
    {
        //on collision with a powerup -> collect logic
        if (other.TryGetComponent<Powerup>(out var powerup))
        {
            powerup.Collect(gameObject);
        }
    }
}

