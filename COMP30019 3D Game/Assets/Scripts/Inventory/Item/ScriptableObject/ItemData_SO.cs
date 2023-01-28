using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType{Useable, Weapon, Shield}
[CreateAssetMenu(fileName = "New_Item", menuName = "Inventory/Item Data")]
public class ItemData_SO : ScriptableObject {
    public ItemType itemType;
    public string itemName;
    public Sprite itemIcon;
    public int itemAmount;

    [TextArea]
    public string description;
    
    public bool stackable;

    [Header("Useable Item")]
    public UseableItemData_SO useableData;
    
    [Header("Weapon")]
    public GameObject weaponPrefab;
    public AttackData_SO weaponData;
    public AnimatorOverrideController weaponAnimator;

    [Header("Shield")]
    public GameObject shieldPrefab;
    public ShieldData_SO shieldData;
}
