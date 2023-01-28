using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield", menuName = "Character Stats/ShieldData")]
public class ShieldData_SO : ScriptableObject
{
    [Header("Shield Info")]
    public int health;
    public int defence;
}
