using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;

    public CharacterData_SO templateCharacterData;
    [HideInInspector]
    public CharacterData_SO characterData;

    public AttackData_SO templateAttackData;

    [HideInInspector]
    public AttackData_SO attackData;


    [Header("Weapon")]
    public Transform weaponSlot;
    [Header("Shield")]
    public Transform shieldSlot;

    [Header("Skill")]
    [HideInInspector]
    public bool isReturnDamageAndImmunity = false;

    [HideInInspector]
    public bool isCritical;
    [HideInInspector]
    public bool hasFreezeSkill = false;
    [HideInInspector]
    public bool hasRestoreHealthSkill = false;
    [HideInInspector]
    public bool hasBurningSkill = false;
    [HideInInspector]
    public bool hasImmunitySkill = false;
    private RuntimeAnimatorController baseAnimator;

    void Awake() {
        if (templateCharacterData != null) characterData = Instantiate(templateCharacterData);
        if (templateAttackData != null) attackData = Instantiate(templateAttackData);
        baseAnimator = GetComponent<Animator>().runtimeAnimatorController;
    }

#region Read from Data_SO

    public int MaxHealth{
        get { if (characterData != null) return characterData.maxHealth; else return 0; }
        set { characterData.maxHealth = value; }
    }
    public int CurrentHealth{
        get { if (characterData != null) return characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }
    public int BaseDefence{
        get { if (characterData != null) return characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }
    public int CurrentDefence{
        get { if (characterData != null) return characterData.currentDefence; else return 0; }
        set { characterData.currentDefence = value; }
    }

    public Vector3 CharacterPosition {
        get { if (characterData != null) return characterData.characterPosition; else return new Vector3(0.0f, 0.0f, 0.0f); }
        set { characterData.characterPosition = value; }
    }
    public Quaternion CharacterRotation {
        get { if (characterData != null) return characterData.characterRotation; else return new Quaternion(0.0f, 0.0f, 0.0f, 0.0f); }
        set { characterData.characterRotation = value; }
    }

#endregion

#region Character Combat

    public void GetDamage(CharacterStats attacker, CharacterStats defender)
    {
        if (isReturnDamageAndImmunity) {
            ReturnDamageAndImmunity(attacker, defender);
            return;
        }
        int damage = Mathf.Max(attacker.CurrentDamage(attacker) - defender.CurrentDefence, 1);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        if (attacker.isCritical) 
        {
            defender.GetComponent<Animator>().SetTrigger("BeAttacked");
        }

        refreshHealthBar();

        // Add experience for upgrade
        if (CurrentHealth <= 0) {
            attacker.characterData.UpdateExp(attacker, characterData.killExperience);
            if (characterData.isBoss) {
                if (characterData.isIce) attacker.hasFreezeSkill = true;
                if (characterData.isFire) attacker.hasBurningSkill = true;
                if (characterData.isRock) attacker.hasImmunitySkill = true;
                if (characterData.isForest) attacker.hasRestoreHealthSkill = true;
            }
        }
    }

    public void refreshHealthBar() {
        // Update the health bar display
        if(UpdateHealthBarOnAttack!=null) {
            UpdateHealthBarOnAttack.Invoke(CurrentHealth, MaxHealth);
            //if(CurrentHealth<=0) UpdateHealthBarOnAttack = null;
        }
    }

    private int CurrentDamage(CharacterStats attacker)
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (attacker.isCritical) {
            coreDamage *= attackData.criticalMultiplier;
        } 
        return (int)coreDamage;
    }
#endregion

#region Equip Weapon & Shield

    public void ChangeWeapon(ItemData_SO weapon) {
        UnEquipWeapon();
        EquipWeapon(weapon);
    }

    public void EquipWeapon(ItemData_SO weapon) {
        if (weapon.weaponPrefab != null) {
            Instantiate(weapon.weaponPrefab, weaponSlot);
        }

        // refresh the player data
        attackData.ApplyWeaponData(attackData, weapon.weaponData);
        // change the animation
        GetComponent<Animator>().runtimeAnimatorController = weapon.weaponAnimator;

    }

    public void UnEquipWeapon() {
        if (weaponSlot.transform.childCount != 0) {
            for (int i = 0; i<weaponSlot.transform.childCount; i++) {
                Destroy(weaponSlot.transform.GetChild(i).gameObject);
            }
        }
        attackData.DropWeaponData(attackData);
        //change the animation
        GetComponent<Animator>().runtimeAnimatorController = baseAnimator;
    }

    public void ChangeShield(ItemData_SO shield) {
        UnEquipShield();
        EquipShield(shield);
    }

    public void EquipShield(ItemData_SO shield) {
        if (shield.shieldPrefab != null) {
            Instantiate(shield.shieldPrefab, shieldSlot);
        }

        // refresh the player data
        characterData.ApplyShieldData(shield.shieldData);

    }

    public void UnEquipShield() {
        if (shieldSlot.transform.childCount != 0) {
            for (int i = 0; i<shieldSlot.transform.childCount; i++) {
                Destroy(shieldSlot.transform.GetChild(i).gameObject);
            }
        }
        characterData.DropShieldData();
    }

#endregion

#region Applay Data Change
public void RestoreHealth(int amount) {
    CurrentHealth += (int)(amount * MaxHealth / 100);
    if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
}
public void AddHealth(int amount) {
    MaxHealth += (int)(amount * MaxHealth /100);
    RestoreHealth(100);
}
public void AddAttack(int amount) {
    attackData.minDamage += Mathf.Max((int)(amount * attackData.minDamage/100), 1);
    attackData.maxDamage += Mathf.Max((int)(amount * attackData.maxDamage/100), 1);
}
public void AddDefence(int amount) {
    CurrentDefence +=  Mathf.Max((int)(amount * CurrentDefence/100), 1);
    BaseDefence += Mathf.Max((int)(amount * BaseDefence/100), 1);
}
#endregion

#region Skill
public void ReturnDamageAndImmunity(CharacterStats attacker, CharacterStats defender) {
    //attacker.GetDamage(defender, attacker);
    int damage = Mathf.Max(attacker.CurrentDamage(attacker) - defender.CurrentDefence, 1);
    attacker.CurrentHealth = Mathf.Max(attacker.CurrentHealth - damage, 0);

    if (attacker.isCritical) 
    {
        attacker.GetComponent<Animator>().SetTrigger("BeAttacked");
    }

    attacker.refreshHealthBar();
        
    // Add experience for upgrade
    if (attacker.CurrentHealth <= 0) defender.characterData.UpdateExp(defender, attacker.characterData.killExperience);
}

public void RestoreHealthSkill() {
    CurrentHealth += Mathf.Max((int)(0.5 * MaxHealth * Time.deltaTime), 1);
    if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
}

public void SlowlyRestoreHealth() {
    CurrentHealth += Mathf.Max((int)(0.5 * MaxHealth * Time.deltaTime), 1);
    if (CurrentHealth > MaxHealth) {
        CurrentHealth = MaxHealth;
    }
}
#endregion
}
