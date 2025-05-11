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
    [SerializeField] private float switchtime = 3f;
    [SerializeField] private float slowtimeFromJumppad = 4f;
    [SerializeField] private float slowtimeFromBash = 2f;
    [SerializeField] private float maxSlowtime = 15f;
    [SerializeField] private bool keepSlowtime = true;


    //[SerializeField] private float maxFuel = 100f;

    


    // Public readonly properties
    public int MaxHp => maxHp;
    public int MaxArrowCount => maxArrowCount;
    //public float MaxFuel => maxFuel;
    public float Switchtime => switchtime;
}
