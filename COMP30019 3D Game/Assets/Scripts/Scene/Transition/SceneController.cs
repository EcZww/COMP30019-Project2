using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class SceneController : Singleton<SceneController>
{
    public GameObject playerPrefab;
    GameObject player;
    NavMeshAgent playerAgent;
    private Vector3 storePosition;
    private Quaternion storeRotation;
    public GameObject keyboardTutorial;
    private bool keyboardTutorialOpen = false;
    public GameObject smileBeforeTutorial;
    private bool smileBeforeTutorialOpen = false;
    public GameObject smileAfterTutorial;
    private bool smileAfterTutorialOpen = false;
    public GameObject turtleBeforeTutorial;
    private bool turtleBeforeTutorialOpen = false;
    public GameObject turtleAfterTutorial;
    private bool turtleAfterTutorialOpen = false;
    public GameObject gruntBeforeTutorial;
    private bool gruntBeforeTutorialOpen = false;
    public GameObject gruntAfterTutorial;
    private bool gruntAfterTutorialOpen = false;
    private bool noRepeatSlime = true;
    private bool noRepeatGrunt = true;
    private bool noRepeatTurtle = true;
    [HideInInspector]
    public bool slimeDead = false;
    [HideInInspector]
    public bool turtleDead = false;
    [HideInInspector]
    public bool gruntDead = false;

    float beginning;

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    void Update() {
        if (SceneManager.GetActiveScene().name == "InitialScene") SetActiveCanvas();
        if (slimeDead && noRepeatSlime) {
            smileBeforeTutorialOpen = false;
            if (smileAfterTutorial!=null) smileAfterTutorialOpen = true;
            slimeDead = false;
            noRepeatSlime = false;
        }
        if (turtleDead && noRepeatTurtle) {
            turtleBeforeTutorialOpen = false;
            if (turtleAfterTutorial!=null) turtleAfterTutorialOpen = true;
            turtleDead = false;
            noRepeatTurtle = false;
        }
        if (gruntDead && noRepeatGrunt) {
            gruntBeforeTutorialOpen = false;
            if (gruntAfterTutorial!=null) gruntAfterTutorialOpen = true;
            gruntDead = false;
            noRepeatGrunt = false;
        }
    }

    void SetActiveCanvas() {
        smileBeforeTutorial.SetActive(smileBeforeTutorialOpen);
        smileAfterTutorial.SetActive(smileAfterTutorialOpen);
        turtleBeforeTutorial.SetActive(turtleBeforeTutorialOpen);
        turtleAfterTutorial.SetActive(turtleAfterTutorialOpen);
        gruntBeforeTutorial.SetActive(gruntBeforeTutorialOpen);
        gruntAfterTutorial.SetActive(gruntAfterTutorialOpen);
        keyboardTutorial.SetActive(keyboardTutorialOpen);
    }

    public void TransitionToDestination(TransitionPoint transitionPoint) {
        switch (transitionPoint.transitionType) {
            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
        }
    }

    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag) {
        // Store the player data
        SaveDataManager.Instance.SavePlayerData();
        //InventoryManager.Instance.SaveData();
        if (SceneManager.GetActiveScene().name != sceneName) {
            gruntBeforeTutorialOpen = false;
            gruntAfterTutorialOpen = false;
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            // Read the player data
            SaveDataManager.Instance.LoadPlayerData();
            // Play the tutorial video
            
            var videoPlayer = GameObject.FindWithTag("Video");
            videoPlayer.SetActive(true);
            videoPlayer.GetComponentInChildren<UnityEngine.Video.VideoPlayer>().Play();
            beginning = Time.time;
            StartCoroutine(waitmethod(videoPlayer));
            
            yield break;
        } else {
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true;
            // play "defeat enemies could drop weapon" tutorial
            if (sceneName == "InitialScene" && destinationTag == TransitionDestination.DestinationTag.A) {
                keyboardTutorialOpen = false;
                smileBeforeTutorialOpen = true;
            }
            if (sceneName == "InitialScene" && destinationTag == TransitionDestination.DestinationTag.B) {
                smileBeforeTutorialOpen = false;
                smileAfterTutorialOpen = false;
                turtleBeforeTutorialOpen = true;
            }
            if (sceneName == "InitialScene" && destinationTag == TransitionDestination.DestinationTag.C) {
                turtleBeforeTutorialOpen = false;
                turtleAfterTutorialOpen = false;
                gruntBeforeTutorialOpen = true;
            }
            yield return null;
        }  
    }

    IEnumerator waitmethod(GameObject videoPlayer) {
        while (Time.time - beginning < videoPlayer.GetComponentInChildren<UnityEngine.Video.VideoPlayer>().length) {
            yield return null;
        }
        videoPlayer.SetActive(false);
    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destination) {
        var entrances = FindObjectsOfType<TransitionDestination>();
        for (int i=0; i<entrances.Length; i++) {
            if (entrances[i].destinationTag == destination) return entrances[i];
        }
        return null;
    }

    // load tutorial level for new game
    public void TransitionToTutorialLevel() {
        StartCoroutine(LoadLevel("InitialScene"));
    }

    // load main menu
    public void TransitionToMenu() {
        StartCoroutine(LoadMenu());
    }

    // load level from previously saved data
    public bool TransitionToLoadGame() {
        if (SaveDataManager.Instance.LoadPlayerData() == false) return false;
        storePosition = GameManager.Instance.playerStats.CharacterPosition;
        storeRotation = GameManager.Instance.playerStats.CharacterRotation;
        StartCoroutine(ContinueLevel(SaveDataManager.Instance.SceneName));
        return true;
    }

    // go back to main menu 
    IEnumerator LoadMenu() {
        yield return SceneManager.LoadSceneAsync("MenuScene");
        yield break;
    }

    // go to specific scene
    IEnumerator LoadLevel(string scene) {
        if (scene != "") {
          
            yield return SceneManager.LoadSceneAsync(scene);
            // Debug.Log(GameManager.Instance.GetEntrance());
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
            
            // save player data 
            SaveDataManager.Instance.SavePlayerData();
            smileBeforeTutorialOpen = false;
            smileAfterTutorialOpen = false;
            turtleBeforeTutorialOpen = false;
            turtleAfterTutorialOpen = false;
            gruntBeforeTutorialOpen = false;
            gruntAfterTutorialOpen = false;
            keyboardTutorialOpen = true ;
            noRepeatSlime = true;
            noRepeatTurtle = true;
            noRepeatGrunt = true;
            yield break;
        }
    }

    IEnumerator ContinueLevel(string scene) {
        if (scene != "") {
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, storePosition, storeRotation);
            
            // save player data 
            SaveDataManager.Instance.SavePlayerData();

            yield break;
        }
    }
}
