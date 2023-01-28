using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Usable Item", menuName = "Inventory/Useable Item Data")]
public class UseableItemData_SO : ScriptableObject {
    public int restoreHealth;
    public int healthPoint;
    public int attackPoint;
    public int defencePoint;
}
