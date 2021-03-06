using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterStats : MonoBehaviour
{
    public int level = 10;
    public float maxHealth, currentHealth;
    public float maxStamina, currentStamina;
    public float maxBreath, currentBreath;
    public bool isDead;


    public virtual void TakeDamage(float damage)
    {

        if (isDead)
            return;

        currentHealth = currentHealth - damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
    }

}