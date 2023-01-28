using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomlyGenerateSupplies : MonoBehaviour
{
    [System.Serializable]
    public class LootSupplies {
        public GameObject item;
        [Range(0,1)]
        public float weight;
    }
    public LootSupplies[] supplies;
    public int randomGenerateNum;
    private int randomIndex;
    private float randomPoisitionX;
    private float randomPoisitionZ;
    private float currentValue;
    public int RandomGenerateCooldown = 300;
    private float coolDownLessTime;
    public Cooldown cooldownUI;


    void OnEnable()
    {
        for (int i = 0; i < (int)(randomGenerateNum/2); i++) {
            randomGenerateSupply();
        }
        coolDownLessTime = 900;
        cooldownUI.cooldownTime = RandomGenerateCooldown;
        cooldownUI.hasSkill = true;
    }

    void Update() {
        coolDownLessTime -= Time.deltaTime;
        cooldownUI.cooldownTimeLess = coolDownLessTime;
        if (coolDownLessTime < 0) {
            for (int i = 0; i < randomGenerateNum; i++) {
                randomGenerateSupply();
            }
            coolDownLessTime = RandomGenerateCooldown;
        }
    }

    void randomGenerateSupply() {
        randomIndex = Random.Range(0, supplies.Length);
        currentValue = Random.value;
        if (currentValue <= supplies[randomIndex].weight) {
            randomPoisitionX = Random.Range(0, 400);
            randomPoisitionZ = Random.Range(-200, 200);
            GameObject supply = Instantiate<GameObject>(supplies[randomIndex].item);
            supply.transform.position = new Vector3(randomPoisitionX, 50f, randomPoisitionZ);
        }
    }
}
