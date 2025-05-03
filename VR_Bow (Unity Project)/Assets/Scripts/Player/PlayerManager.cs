using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IDamageable
{
   

    [SerializeField] private int hp;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int arrowCount;
    [SerializeField] private int maxArrowCount = 6;
    [SerializeField] private float slowTime;
    [SerializeField] private float fuel;
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] PlayerState state;



   
    public int Hp 
    {
        get { return hp; }
        set { hp = Mathf.Min(value, maxHp); } //Make sure that health never exceeds max Health
    }
    public int ArrowCount
    {
        get { return arrowCount; } 
        set {  arrowCount = Mathf.Clamp(value, 0, maxArrowCount); }
    }

    public float SlowTime 
    { 
        get { return slowTime; } 
        set { slowTime = value; } 
    }

    public float Fuel
    {
        get { return fuel; }
        set { fuel = Mathf.Clamp(value, 0, maxFuel); }
    }

    public PlayerState State
    {
        get { return state; }
        set { state = value; }
    }


    public void TakeDamage(int amount)
    {
        Hp -= amount;
    }
}
