using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    Button newGameBtn;
    Button continueBtn;
    Button quitBtn;
    public VolumeSlider volumeSlider;
    public BrightnessSlider brightnessSlider;
    public GameObject deadCanvas;

    void Awake() {
        // Debug.Log(transform.childCount);
        newGameBtn = transform.GetChild(0).GetComponent<Button>();
        continueBtn = transform.GetChild(1).GetComponent<Button>();
        quitBtn = transform.GetChild(4).GetComponent<Button>();
        
        newGameBtn.onClick.AddListener(NewGame);
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuitGame);
    }

    void Start() {
        SaveDataManager.Instance.LoadVolume();
        volumeSlider.volumeSlider.value = GameManager.Instance.volume;
        SaveDataManager.Instance.LoadBrightness();
        brightnessSlider.brightnessSlider.value = GameManager.Instance.brightness;
    }

    void OnEnable() {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        if (SaveDataManager.Instance.playerDead) StartCoroutine(waitDeadCanvasDone());
    }

    IEnumerator waitDeadCanvasDone() {
        deadCanvas.SetActive(true);
        yield return new WaitForSeconds(5f);
        deadCanvas.SetActive(false);
    }

    

    public void NewGame() {
        float volume = SaveDataManager.Instance.LoadVolume();
        float brightness = SaveDataManager.Instance.LoadBrightness();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetFloat("Volume", GameManager.Instance.volume);
        PlayerPrefs.SetFloat("Brightness", GameManager.Instance.brightness);
        // change the scene
        SceneController.Instance.TransitionToTutorialLevel();
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ContinueGame() {
        // redirect to new game if no previous game played
        //change the scene to where the loaded data is 
        if (SceneController.Instance.TransitionToLoadGame() == false) NewGame();
               
    }

    public void QuitGame() {
        Application.Quit();
        // Debug.Log("Quit Game!");
    }

}
