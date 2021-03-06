using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Consumables/Flask")]
public class FlaskItem : ConsumableItemStack
{
    [Header("Flask type")]
    public bool healthFlask, staminaFlask, breathFlask;
    [Header("Heal Amounts")]
    public int healthAmount, staminaAmount, breathAmount;
    [Header("Recovery FX")]
    public GameObject recoveryFX;

    public override void AtteptToConsumeItem(AnimatorManager playerAnimatorManager, WeaponSlotManager weaponSlotManager, PlayerEffectsManager playerEffectsManager)
    {
        base.AtteptToConsumeItem(playerAnimatorManager, weaponSlotManager, playerEffectsManager);
        GameObject flask = Instantiate(itemModel, weaponSlotManager.leftHandSlot.transform);
        playerEffectsManager.currentParticleFX = recoveryFX;
        playerEffectsManager.healthAmount = healthAmount;
        playerEffectsManager.staminaAmount = staminaAmount;
        playerEffectsManager.breathAmount = breathAmount;
        playerEffectsManager.instantiatedFXModel = flask;
        weaponSlotManager.leftHandSlot.UnloadWeapon();
    }

}
