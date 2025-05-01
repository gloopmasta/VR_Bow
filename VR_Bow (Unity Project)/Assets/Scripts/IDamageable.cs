using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    int Hp { get; set; }

    void TakeDamage(int amount);

}
