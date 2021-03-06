using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    WeaponSlotManager weaponSlotManager;
    QuickSlotsUI quickSlotsUI;

    public WeaponItemStack rightWeapon, leftWeapon, unarmedWeapon;
    public WeaponItemStack[] weaponsInRightHandSlots = new WeaponItemStack[1];
    public WeaponItemStack[] weaponsInLeftHandSlots = new WeaponItemStack[1];
    public ConsumableItemStack currentConsumable,unarmedConsumable;
    public ConsumableItemStack[] consumableInQuickSlots = new ConsumableItemStack[3];

    public SpellItemStack currentSpell, unarmedSpell;
    public SpellItemStack[] spellInQuickSlots = new SpellItemStack[3];

    public int currentRightWeaponIndex = 0;
    public int currentLeftWeaponIndex = 0;
    public int currentConsumableIndex = 0;
    public int currentSpellIndex = 0;

    public List<WeaponItemStack> weaponsInventory;
    public List<KeyItemStack> keyItemsInventory;
    public List<CraftingItemStack> craftingItemInventory;
    public List<SpellItemStack> spellInventory;

    [Header("Current Equipment")]
    public HelmetEquipment currentHelmetEquipment;
    public TorsoEquipment currentTorsoEquipment;
    public LegEquipment currentLegEquipment;
    public HandEquipment currentHandEquipment;



    private void Awake()
    {
        weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
        quickSlotsUI = GetComponentInChildren<QuickSlotsUI>();
    }

    private void Start()
    {
        rightWeapon = weaponsInRightHandSlots[0];
        leftWeapon = weaponsInLeftHandSlots[0];
        currentConsumable = consumableInQuickSlots[0];
        currentSpell = spellInQuickSlots[0];

        weaponSlotManager.LoadWeaponOnSlot(rightWeapon, false);
        weaponSlotManager.LoadWeaponOnSlot(leftWeapon, true);
        quickSlotsUI.UpdateConsumableQuickSlotsUi(currentConsumable);
        quickSlotsUI.UpdateSpellQuickSlotsUi(currentSpell);


    }

    //This may be the grossest bit of code ive ever wrote,but "It just works" ~ Vince Comaroto 1:44 am 12/8/21
    public void AddToCraftingStack(CraftingItemStack itemToAdd)
    {
        if (craftingItemInventory.Count <= 0)
        {
            craftingItemInventory.Add((CraftingItemStack)itemToAdd.GetCopy());
            itemToAdd.currentSize = 0;
            return;
        }
        else
        {
            for (int i = craftingItemInventory.Count - 1; i >= 0; i--)
            {
                CraftingItemStack craft = craftingItemInventory[i];
                if (!craft.IsFull())
                {
                    if (craft.itemType == itemToAdd.itemType)
                    {
                        if (craft.currentSize + itemToAdd.currentSize > craft.itemType.stacksTo)
                        {
                            int difference = (itemToAdd.currentSize + craft.currentSize) - craft.itemType.stacksTo;
                            craft.currentSize = craft.itemType.stacksTo;
                            CraftingItemStack diffStack = (CraftingItemStack)itemToAdd.GetCopy();
                            diffStack.currentSize = difference;
                            craftingItemInventory.Add(diffStack);
                            return;
                        }
                        else
                        {
                            craft.currentSize += itemToAdd.currentSize;
                            return;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < craftingItemInventory.Count; j++)
                        {
                            if (craftingItemInventory[j].itemType == itemToAdd.itemType)
                            {
                                if (craftingItemInventory[j].currentSize + itemToAdd.currentSize > craftingItemInventory[j].itemType.stacksTo)
                                {
                                    int difference = (itemToAdd.currentSize + craftingItemInventory[j].currentSize) - craftingItemInventory[j].itemType.stacksTo;
                                    craftingItemInventory[j].currentSize = craftingItemInventory[j].itemType.stacksTo;
                                    CraftingItemStack diffStack = (CraftingItemStack)itemToAdd.GetCopy();
                                    diffStack.currentSize = difference;
                                    craftingItemInventory.Add(diffStack);
                                    return;
                                }
                                else
                                {
                                    craftingItemInventory[j].currentSize += itemToAdd.currentSize;
                                    return;
                                }
                            }
                        }
                        craftingItemInventory.Add((CraftingItemStack)itemToAdd.GetCopy());
                        return;
                    }
                }
                else
                {
                    craftingItemInventory.Add((CraftingItemStack)itemToAdd.GetCopy());
                    return;
                }
            }
        }
    }

    public void ChangeRightWeapon()
    {
        currentRightWeaponIndex = currentRightWeaponIndex + 1;

        if (currentRightWeaponIndex == 0 && weaponsInRightHandSlots[0] != null)
        {
            rightWeapon = weaponsInRightHandSlots[currentRightWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponsInRightHandSlots[currentRightWeaponIndex], false);
        }
        else if (currentRightWeaponIndex == 0 && weaponsInRightHandSlots[0] == null)
        {
            currentRightWeaponIndex = currentRightWeaponIndex + 1;
        }

        else if (currentRightWeaponIndex == 1 && weaponsInRightHandSlots[1] != null)
        {
            rightWeapon = weaponsInRightHandSlots[currentRightWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponsInRightHandSlots[currentRightWeaponIndex], false);
        }
        else if (currentRightWeaponIndex == 1 && weaponsInRightHandSlots[1] == null)
        {
            currentRightWeaponIndex = currentRightWeaponIndex + 1;
        }

        if (currentRightWeaponIndex > weaponsInRightHandSlots.Length - 1)
        {
            currentRightWeaponIndex = -1;
            rightWeapon = unarmedWeapon;
            weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, false);
        }
    }
    public void ChangeLeftWeapon()
    {
        currentLeftWeaponIndex = currentLeftWeaponIndex + 1;

        if (currentLeftWeaponIndex == 0 && weaponsInLeftHandSlots[0] != null)
        {
            leftWeapon = weaponsInLeftHandSlots[currentLeftWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponsInLeftHandSlots[currentLeftWeaponIndex], true);
        }
        else if (currentLeftWeaponIndex == 0 && weaponsInLeftHandSlots[0] == null)
        {
            currentLeftWeaponIndex = currentLeftWeaponIndex + 1;
        }

        else if (currentLeftWeaponIndex == 1 && weaponsInLeftHandSlots[1] != null)
        {
            leftWeapon = weaponsInLeftHandSlots[currentLeftWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponsInLeftHandSlots[currentLeftWeaponIndex], true);
        }
        else if (currentLeftWeaponIndex == 1 && weaponsInLeftHandSlots[1] == null)
        {
            currentLeftWeaponIndex = currentLeftWeaponIndex + 1;
        }

        if (currentLeftWeaponIndex > weaponsInLeftHandSlots.Length - 1)
        {
            currentLeftWeaponIndex = -1;
            leftWeapon = unarmedWeapon;
            weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, true);
        }
    }
    public void ChangeCurrentConsumable()
    {
        currentConsumableIndex = currentConsumableIndex + 1;


        for(int i = 0; i< consumableInQuickSlots.Length; i++)
        {

            if (currentConsumableIndex == i && consumableInQuickSlots[i] != null)
            {
                currentConsumable = consumableInQuickSlots[currentConsumableIndex];
            }
            else if (currentConsumableIndex == i && consumableInQuickSlots[i] == null)
            {
                currentConsumableIndex = currentConsumableIndex + 1;
            }

        }

        if (currentConsumableIndex > consumableInQuickSlots.Length - 1)
        {
            currentConsumableIndex = -1;
            currentConsumable = unarmedConsumable;
        }
        quickSlotsUI.UpdateConsumableQuickSlotsUi(currentConsumable);
    }
    public void ChangeCurrentSpell()
    {
        currentSpellIndex = currentSpellIndex + 1;


        for (int i = 0; i < spellInQuickSlots.Length; i++)
        {

            if (currentSpellIndex == i && spellInQuickSlots[i] != null)
            {
                currentSpell = spellInQuickSlots[currentSpellIndex];
            }
            else if (currentSpellIndex == i && spellInQuickSlots[i] == null)
            {
                currentSpellIndex = currentSpellIndex + 1;
            }

        }

        if (currentSpellIndex > spellInQuickSlots.Length - 1)
        {
            currentSpellIndex = -1;
            currentSpell = unarmedSpell;
        }
        quickSlotsUI.UpdateSpellQuickSlotsUi(currentSpell);
    }

}
