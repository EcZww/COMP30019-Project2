using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : Singleton<InventoryManager>
{
    public class DragData {
        public SlotHolder originalHolder;
        public RectTransform originalParent;
    }
    //add the template for storing the data
    [Header("Inventory Data")]
    public InventoryData_SO inventoryTemplateData;
    [HideInInspector]
    public InventoryData_SO inventoryData;

    public InventoryData_SO inventoryTemplateActionData;
    [HideInInspector]
    public InventoryData_SO actionData;

    public InventoryData_SO inventoryTemplateEquipmentData;
    [HideInInspector]
    public InventoryData_SO equipmentData;

    [Header("Containers")]
    public ContainerUI inventoryUI;
    public ContainerUI actionUI;
    public ContainerUI equipmentUI;

    [Header("DragCanvas")]
    public Canvas dragCanvas;
    public DragData currentDrag;
    [Header("UI Panel")]
    public GameObject bagPanel;
    public GameObject stasPanel;

    [Header("Stas Text")]
    public Text healthText;
    public Text attackText;
    public Text defenceText;
    public Text criticalText;
    [Header("Tooltip")]
    public ItemTip itemTip;


    public bool isOpen = false;
    
    protected override void Awake() {
        base.Awake();
        if (inventoryTemplateData != null) inventoryData = Instantiate(inventoryTemplateData);
        if (inventoryTemplateActionData != null) actionData = Instantiate(inventoryTemplateActionData);
        if (inventoryTemplateEquipmentData != null) equipmentData = Instantiate(inventoryTemplateEquipmentData);
    }


    void Start() {
        LoadData();
        inventoryUI.RefreshUI();
        actionUI.RefreshUI();
        equipmentUI.RefreshUI();
    }


    void Update() {
        if (GameManager.Instance.playerStats!=null) {
            int MaxHealth = GameManager.Instance.playerStats.MaxHealth;
            int minDamage = GameManager.Instance.playerStats.attackData.minDamage;
            int CurrentDefence = GameManager.Instance.playerStats.CurrentDefence;
            float criticalChance = GameManager.Instance.playerStats.attackData.criticalChance;
            InventoryManager.Instance.UpdateStatsText(MaxHealth, minDamage, CurrentDefence, criticalChance);
        }
        if(Input.GetKeyDown(KeyCode.Tab)) {
            isOpen = !isOpen;
            bagPanel.SetActive(isOpen);
            stasPanel.SetActive(isOpen);
            if (isOpen) {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

    }

    public void UpdateStatsText(int health, int attack, int defence, float critical) {
        healthText.text = health.ToString();
        attackText.text = attack.ToString();
        defenceText.text = defence.ToString();
        criticalText.text = (critical * 100).ToString() + "%";
    }

    #region check the drag item whether in each slot area
    public bool CheckInInventory(Vector3 position) {
        for (int i = 0; i < inventoryUI.slotHolders.Length; i++) {
            RectTransform t = inventoryUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position)) {
                return true;
            }
        }
        return false;
    }

    public bool CheckInAction(Vector3 position) {
        for (int i = 0; i < actionUI.slotHolders.Length; i++) {
            RectTransform t = actionUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position)) {
                return true;
            }
        }
        return false;
    }

    public bool CheckInEquipment(Vector3 position) {
        for (int i = 0; i < equipmentUI.slotHolders.Length; i++) {
            RectTransform t = equipmentUI.slotHolders[i].transform as RectTransform;

            if (RectTransformUtility.RectangleContainsScreenPoint(t, position)) {
                return true;
            }
        }
        return false;
    }

    #endregion
    
    #region Save Data
    public void SaveData() {
        SaveDataManager.Instance.Save(inventoryData, inventoryData.name);
        SaveDataManager.Instance.Save(actionData, actionData.name);
        SaveDataManager.Instance.Save(equipmentData, equipmentData.name);
    }

    public void LoadData() {
        SaveDataManager.Instance.Load(inventoryData, inventoryData.name);
        SaveDataManager.Instance.Load(actionData, actionData.name);
        SaveDataManager.Instance.Load(equipmentData, equipmentData.name);
    }
    #endregion
}
