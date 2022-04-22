using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellItem : Item
{
    public GameObject spellWarmUpFX;
    public GameObject spellCastFX;
    public string spellAnimation;

    [Header("Spell Cost")]
    public int breathCost;

    [Header("Spell Type")]
    public bool isFaithSpell;
    public bool isMagicSpell;
    public bool isPyroSpell;

    [Header("Spell Description")]
    [TextArea]
    public string spellDescription;

    public virtual void AttemptToCastSpell(AnimatorManager animatorHandler, PlayerStats playerStats)
    {
    }

    public virtual void SuccessfullyCastSpell(AnimatorManager animatorHandler, PlayerStats playerStats)
    {
        playerStats.TakeBreathDamage(breathCost);

    }
}