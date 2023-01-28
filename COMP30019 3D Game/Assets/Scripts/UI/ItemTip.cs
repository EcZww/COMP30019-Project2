using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemTip : MonoBehaviour
{
    public Text itemNameText;
    public Text itemInfoText;


    public void SetupItemTip(ItemData_SO item) {
        itemNameText.text = item.itemName;
        itemInfoText.text = item.description;
    }

}
