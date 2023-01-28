using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SlotType {BAG, WEAPON, SHIELD, Action}
public class SlotHolder : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler{
    public SlotType slotType;
    public ItemUI itemUI;

    void OnEnable() {
        UpdateItem();
    }

    void OnDisable() {
        InventoryManager.Instance.itemTip.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.clickCount%2 == 0) {
            useItem();
        }
    }

    public void useItem() {
        if (itemUI.GetItem() == null) return;
        if (itemUI.GetItem().itemType == ItemType.Useable && itemUI.Bag.items[itemUI.Index].amount > 0) {
            GameManager.Instance.playerStats.RestoreHealth(itemUI.GetItem().useableData.restoreHealth);
            if (itemUI.GetItem().useableData.healthPoint != 0)
                GameManager.Instance.playerStats.AddHealth(itemUI.GetItem().useableData.healthPoint);
            GameManager.Instance.playerStats.AddAttack(itemUI.GetItem().useableData.attackPoint);
            GameManager.Instance.playerStats.AddDefence(itemUI.GetItem().useableData.defencePoint);
            itemUI.Bag.items[itemUI.Index].amount -= 1;
        }
        UpdateItem();
    }

    public void UpdateItem() {
        switch (slotType) {
            case SlotType.BAG:
                itemUI.Bag = InventoryManager.Instance.inventoryData;
                break;
            case SlotType.WEAPON: 
                itemUI.Bag = InventoryManager.Instance.equipmentData;
                // Change the weapon || equip the weapon
                if (itemUI.GetItem() != null) {
                    GameManager.Instance.playerStats.ChangeWeapon(itemUI.GetItem());
                } else {
                    GameManager.Instance.playerStats.UnEquipWeapon();
                }
                break;
            case SlotType.SHIELD: 
                itemUI.Bag = InventoryManager.Instance.equipmentData;
                if (itemUI.GetItem() != null) {
                    GameManager.Instance.playerStats.ChangeShield(itemUI.GetItem());
                } else {
                    GameManager.Instance.playerStats.UnEquipShield();
                }
                break;
            case SlotType.Action: 
                itemUI.Bag = InventoryManager.Instance.actionData;
                break;
        }

        if (itemUI.Index != -1) {
            var item = itemUI.Bag.items[itemUI.Index];
            itemUI.SetupItemUI(item.itemData, item.amount);
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if(itemUI.GetItem() != null) {
            InventoryManager.Instance.itemTip.SetupItemTip(itemUI.GetItem());
            InventoryManager.Instance.itemTip.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        InventoryManager.Instance.itemTip.gameObject.SetActive(false);
    }
    

}
