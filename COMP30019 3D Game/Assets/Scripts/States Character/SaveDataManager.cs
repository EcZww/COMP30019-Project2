using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveDataManager : Singleton<SaveDataManager>
{
    // save current scene
    string sceneName = "";
    [HideInInspector]
    public bool playerDead = false;
    private bool noRepeat = true;
    public GameObject saveCanvas;

    public string SceneName {
        get {
            return PlayerPrefs.GetString(sceneName);
        }
    }


    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);

    }

    void Update() {

        if (playerDead && noRepeat) {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "MenuScene") {
                SceneController.Instance.TransitionToMenu();
            }
            playerDead = false;
            noRepeat =false;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "MenuScene") {
                SavePlayerData();
                SceneController.Instance.TransitionToMenu();
            }
        }

        if (Input.GetKeyDown(KeyCode.K)) {
            SavePlayerData();
            StartCoroutine(showCanvas3Sec(saveCanvas));
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            LoadPlayerData();
        }
    }

    IEnumerator showCanvas3Sec(GameObject canvas) {
        canvas.SetActive(true);
        yield return new WaitForSeconds(3f);
        canvas.SetActive(false);
    }

    public void SavePlayerData() {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
        Save(GameManager.Instance.playerStats.attackData, GameManager.Instance.playerStats.attackData.name);
        InventoryManager.Instance.SaveData();
        foreach (GameObject enemy in GameManager.Instance.enemies) {
            enemy.GetComponent<EnemyController>().characterStats.CharacterPosition = enemy.transform.position;
            enemy.GetComponent<EnemyController>().characterStats.CharacterRotation = enemy.transform.rotation;
            if (enemy != null) Save(enemy.GetComponent<EnemyController>().characterStats.characterData, enemy.name);
            
        }
    }

    public bool LoadPlayerData() {
        try {
            //Debug.Log(GameManager.Instance);
            //Debug.Log(GameManager.Instance.playerStats);
            //Debug.Log(GameManager.Instance.playerStats.characterData.name);
            Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
            
            Load(GameManager.Instance.playerStats.attackData, GameManager.Instance.playerStats.attackData.name);
            
            foreach (GameObject enemy in GameManager.Instance.enemies) {
                if (enemy != null) Load(enemy.GetComponent<EnemyController>().characterStats.characterData, enemy.name);
            }
            noRepeat = true;
        }
        catch (NullReferenceException) {
            return false;
        }
        return true;
    }

    public void Save(UnityEngine.Object data, string key) {
        var jsonData = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void Load(UnityEngine.Object data, string key) {
        if (PlayerPrefs.HasKey(key)) {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }

    public void SaveVolume() {
        PlayerPrefs.SetFloat("Volume", GameManager.Instance.volume);
    }

    public void SaveBrightness() {
        PlayerPrefs.SetFloat("Brightness", GameManager.Instance.brightness);
    }

    public float LoadVolume() {
        GameManager.Instance.volume = PlayerPrefs.GetFloat("Volume");
        if (!PlayerPrefs.HasKey("Volume")) GameManager.Instance.volume = 1;
        return GameManager.Instance.volume;
    }

    public float LoadBrightness() {
        GameManager.Instance.brightness = PlayerPrefs.GetFloat("Brightness");
        if (!PlayerPrefs.HasKey("Brightness")) GameManager.Instance.brightness = 1;
        return GameManager.Instance.brightness;
    }

}
