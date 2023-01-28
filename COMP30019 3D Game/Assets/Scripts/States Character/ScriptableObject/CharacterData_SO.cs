using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int baseMaxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("beExecuted")]
    public int killExperience;

    [Header("Level")]
    public int currentLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuf;
    public float moreExperience;

    [Header("Boss")]
    public bool isBoss;
    public bool isIce;
    public bool isFire;
    public bool isRock;
    public bool isForest;

    [Header("Store Info")]
    public Vector3 characterPosition;
    public Quaternion characterRotation;
    public bool isDieing = false;

    public void UpdateExp(CharacterStats attacker, int expValue) 
    {
        currentExp += expValue;
        
        if(currentExp >= baseExp) LevelUp(attacker);
    }

    private void LevelUp(CharacterStats attacker)
    {
        currentLevel++;
        // update your properties
        baseExp += (int)(baseExp * moreExperience);
        int perviousBaseMaxHealth = baseMaxHealth;
        baseMaxHealth += (int)(baseMaxHealth * levelBuf);
        maxHealth = maxHealth + (baseMaxHealth - perviousBaseMaxHealth);
        currentHealth = maxHealth;
        currentExp = 0;
        baseDefence +=  Mathf.Max((int)(baseDefence * levelBuf), 1);
        attacker.attackData.minDamage += Mathf.Max((int)(attacker.attackData.baseMinDamage * levelBuf), 1);
        attacker.attackData.maxDamage += Mathf.Max((int)(attacker.attackData.baseMaxDamage * levelBuf), 1);
        attacker.attackData.baseMinDamage += Mathf.Max((int)(attacker.attackData.baseMinDamage * levelBuf), 1);
        attacker.attackData.baseMaxDamage += Mathf.Max((int)(attacker.attackData.baseMaxDamage * levelBuf), 1);
    }

    public void ApplyShieldData(ShieldData_SO shield) {
        maxHealth = baseMaxHealth + shield.health;
        currentDefence = baseDefence + shield.defence;
    }

    public void DropShieldData() {
        maxHealth = baseMaxHealth;
        currentDefence = baseDefence;
    }
}
