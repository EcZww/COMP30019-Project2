using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : EnemyController
{
    [Header("Skill")]
    public GameObject cannonballPrefab;

    public void Fire() {

            //GetComponent<Animator>().SetTrigger("Attack");

            GameObject cannonball = Instantiate<GameObject>(cannonballPrefab);
            cannonball.transform.position = this.gameObject.transform.GetChild(2).position;
            if (attackTarget!=null) {
                cannonball.GetComponent<CannonBallController>().attackTarget = attackTarget;
                //cannonball.GetComponent<CannonBallController>().targetStats = attackTarget.GetComponent<CharacterStats>();
            }
            cannonball.GetComponent<CannonBallController>().characterStats = characterStats;
            cannonball.GetComponent<CannonBallController>().initialPosition = this.gameObject.transform.GetChild(2).position;
    }
}

