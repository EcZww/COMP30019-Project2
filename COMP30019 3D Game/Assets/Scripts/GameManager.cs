using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector]
    public CharacterStats playerStats;

    public float enemyRespawnTime = 600f;

    [HideInInspector]
    public List<GameObject> enemies = new List<GameObject>();
    [HideInInspector]
    public float volume = 1f;
    [HideInInspector]
    public float brightness = 1f;

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    

    public void RigisterPlayer(CharacterStats player) {
        playerStats = player;
        //playerCameraController = playerStats.transform.GetComponent<PlayerCameraController>();
    }

    public void DieAndRespawn(GameObject enemy) {
        StartCoroutine(Respawn(enemy));
    }

    IEnumerator Respawn(GameObject enemy) {   
        yield return new WaitForSeconds(enemyRespawnTime);
        if (enemy != null) enemy.SetActive(true);
        if (enemy != null) enemy.GetComponent<EnemyController>().characterStats.characterData.isDieing = false;
    }

    public Transform GetEntrance() {
        var entrances = FindObjectsOfType<TransitionDestination>();
        for (int i=0; i<entrances.Length; i++) {
            if (entrances[i].destinationTag == TransitionDestination.DestinationTag.ENTER) {
                return entrances[i].transform;
            }
        }
        return null;
    }
}
