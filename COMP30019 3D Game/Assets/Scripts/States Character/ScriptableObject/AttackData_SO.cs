using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Character Stats/Attack")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    public float skillRange;
    public float coolDown;
    public float baseAttackRange;
    public float baseSkillRange;
    public float baseCoolDown;
    public int baseMinDamage;
    public int baseMaxDamage;
    public int minDamage;
    public int maxDamage;
    public float baseCriticalMultiplier;
    public float criticalMultiplier;
    public float baseCriticalChance;
    public float criticalChance;

    public void ApplyWeaponData(AttackData_SO baseAttack, AttackData_SO weapon) {
        attackRange = weapon.attackRange;
        skillRange = weapon.skillRange;
        coolDown = weapon.coolDown;
        minDamage = baseAttack.baseMinDamage + weapon.minDamage;
        maxDamage = baseAttack.baseMaxDamage + weapon.maxDamage;
        criticalMultiplier = weapon.criticalMultiplier;
        criticalChance = weapon.criticalChance;
    }

    public void DropWeaponData(AttackData_SO baseAttack) {
        attackRange = baseAttack.baseAttackRange;
        skillRange = baseAttack.baseSkillRange;
        coolDown = baseAttack.baseCoolDown;
        minDamage = baseAttack.baseMinDamage;
        maxDamage = baseAttack.baseMaxDamage;
        criticalMultiplier = baseAttack.baseCriticalMultiplier;
        criticalChance = baseAttack.criticalChance;
    }


}
