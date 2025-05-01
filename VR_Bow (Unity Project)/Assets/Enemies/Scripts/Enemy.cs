using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    private void Start()
    {
        GameManager.Instance.RegisterEnemy(gameObject);
    }
    private void OnDestroy()
    {
        GameManager.Instance.UnregisterEnemy(gameObject);
    }

    public int Hp { get; set; } = 2;

    //Basic TakeDamage
    public void TakeDamage(int amount)
    {
        Hp -= amount;
        if (Hp <= 0)
            Die();
    }

    //what happens if HP falls below 0
    protected abstract void Die();
}
