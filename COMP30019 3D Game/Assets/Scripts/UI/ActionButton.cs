using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButton : MonoBehaviour
{
    public KeyCode keyCode;
    private SlotHolder currentSlotHolder;

    void Awake() {
        currentSlotHolder = GetComponent<SlotHolder>();
    }

    void Update() {
        if (Input.GetKeyDown(keyCode) && currentSlotHolder.itemUI.GetItem()) {
            currentSlotHolder.useItem();
        }
    }
}
