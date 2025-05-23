using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    [Header("HP Settings")]
    [SerializeField] private int maxHp = 3;
    [SerializeField] private float invulnerableTime = 2f;

    [Header("Arrow Settings")]
    [SerializeField] private int startingArrowCount = 2;
    [SerializeField] private int maxArrowCount = 6;
    [SerializeField] private int arrowsFromJumppad = 1;
    [SerializeField] private bool keepArrows = true;

    [Header("Slowtime / Switchtime Settings")]
    [SerializeField] private float slowAmount = 0.2f;
    [SerializeField] private float switchtime = 3f;
    [SerializeField] private float slowtimeFromJumppad = 4f;
    [SerializeField] private float slowtimeFromBash = 2f;
    [SerializeField] private float maxSlowtime = 15f;
    [SerializeField] private bool keepSlowtime = true;

    [Header("Bash Settings")]
    [SerializeField] private float bashActiveTime = 0.8f;
    [SerializeField] private float bashCooldown = 1.5f;
    [SerializeField] private int bashDamage = 1;


    //[SerializeField] private float maxFuel = 100f;




    // Public readonly properties
    // Public readonly properties
    public int MaxHp => maxHp;
    public float InvulnerableTime => invulnerableTime;
    public int StartingArrowCount => startingArrowCount;
    public int MaxArrowCount => maxArrowCount;
    public int ArrowsFromJumppad => arrowsFromJumppad;
    public bool KeepArrows => keepArrows;
    public float SlowAmount => slowAmount;
    public float Switchtime => switchtime;
    public float SlowtimeFromJumppad => slowtimeFromJumppad;
    public float SlowtimeFromBash => slowtimeFromBash;
    public float MaxSlowtime => maxSlowtime;
    public bool KeepSlowtime => keepSlowtime;

    public float BashActiveTime => bashActiveTime;
    public float BashCooldown => bashCooldown;
    public int BashDamage => bashDamage; 
}
