using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemUI))]
public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    ItemUI currentItemUI;
    SlotHolder currentHolder;
    SlotHolder targetHolder;

    void Awake() {
        currentItemUI = GetComponent<ItemUI>();
        currentHolder = GetComponentInParent<SlotHolder>();
    }
    public void OnBeginDrag(PointerEventData eventData) {
        InventoryManager.Instance.currentDrag = new InventoryManager.DragData();
        InventoryManager.Instance.currentDrag.originalHolder = GetComponent<SlotHolder>();
        InventoryManager.Instance.currentDrag.originalParent = (RectTransform)transform.parent;
        //record the initial data
        transform.SetParent(InventoryManager.Instance.dragCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData) {
        //follow the movement of mouse
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        // put down the item and exchange data
        // whether point to item
        if (EventSystem.current.IsPointerOverGameObject()) {
            if (InventoryManager.Instance.CheckInInventory(eventData.position) || InventoryManager.Instance.CheckInEquipment(eventData.position) ||
                InventoryManager.Instance.CheckInAction(eventData.position)) {
                if (eventData.pointerEnter.gameObject.GetComponent<SlotHolder>())
                    targetHolder = eventData.pointerEnter.gameObject.GetComponent<SlotHolder>();
                else targetHolder = eventData.pointerEnter.gameObject.GetComponentInParent<SlotHolder>();

                switch(targetHolder.slotType) {
                    case SlotType.BAG:
                            SwapItem();
                        break;
                    case SlotType.WEAPON:
                        if (currentItemUI.Bag.items[currentItemUI.Index].itemData.itemType == ItemType.Weapon)
                            SwapItem();
                        break;
                    case SlotType.SHIELD:
                        if (currentItemUI.Bag.items[currentItemUI.Index].itemData.itemType == ItemType.Shield)
                            SwapItem();
                        break;
                    case SlotType.Action:
                        if (currentItemUI.Bag.items[currentItemUI.Index].itemData.itemType == ItemType.Useable)
                            SwapItem();
                        break;
                }
                currentHolder.UpdateItem();
                targetHolder.UpdateItem();
            }
        }
        transform.SetParent(InventoryManager.Instance.currentDrag.originalParent);
        RectTransform t = transform as RectTransform;
        t.offsetMax = -Vector2.one * 5;
        t.offsetMin = Vector2.one * 5;
    }

    public void SwapItem() {
        var targetItem = targetHolder.itemUI.Bag.items[targetHolder.itemUI.Index];
        var tempItem = currentHolder.itemUI.Bag.items[currentHolder.itemUI.Index];

        bool isSameItem = tempItem.itemData == targetItem.itemData;

        if (isSameItem && targetItem.itemData.stackable) {
            targetItem.amount += tempItem.amount;
            tempItem.itemData = null;
            tempItem.amount = 0;
        } else {
            currentHolder.itemUI.Bag.items[currentHolder.itemUI.Index] = targetItem;
            targetHolder.itemUI.Bag.items[targetHolder.itemUI.Index] = tempItem;
        }
    }
    
}
