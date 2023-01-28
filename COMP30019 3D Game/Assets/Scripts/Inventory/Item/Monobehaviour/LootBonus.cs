using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBonus : MonoBehaviour
{
    [System.Serializable]
    public class LootItem {
        public GameObject item;
        [Range(0,1)]
        public float weight;
    }

    public LootItem[] lootItems;
    private float currentValue;

    public void Spawnloot() {  
        foreach (LootItem loopItem in lootItems) {
            currentValue = Random.value;
            if (currentValue <= loopItem.weight) {
                GameObject obj = Instantiate(loopItem.item);
                obj.transform.position = transform.position + new Vector3(currentValue, currentValue, currentValue)+ Vector3.up * 5;
                obj.transform.Rotate(20.0f, 40.0f, 60.0f, Space.Self);
            }
        }
    }
}
