using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData_SO itemData;
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            //pick up the weapon to the backpack
            InventoryManager.Instance.inventoryData.AddItem(itemData, itemData.itemAmount);
            InventoryManager.Instance.inventoryUI.RefreshUI();

            Destroy(gameObject);
        }
    }
}
