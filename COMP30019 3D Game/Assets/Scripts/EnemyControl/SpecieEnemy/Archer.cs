using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : EnemyController
{/*
    [Header("Skill")]
    public GameObject arrowPrefab;
    public float arrowSpeed;

    public void Archery() {

            //GetComponent<Animator>().SetTrigger("Attack");

            GameObject arrow = Instantiate<GameObject>(arrowPrefab);
            arrow.transform.position = this.gameObject.transform.GetChild(1).GetChild(0).position;
            arrow.transform.rotation = this.gameObject.transform.GetChild(1).GetChild(0).rotation;
            if (attackTarget!=null) {
                arrow.GetComponent<ArrowController>().targetStats = attackTarget.GetComponent<CharacterStats>();
            }
            arrow.GetComponent<ArrowController>().characterStats = characterStats;
            arrow.GetComponent<ArrowController>().initialPosition = this.gameObject.transform.GetChild(1).GetChild(0).position;
            arrow.GetComponent<ArrowController>().arrowSpeed = arrowSpeed;
    }*/

    [Header("Skill")]
    public GameObject arrowPrefab;

    public void Archery() {

            //GetComponent<Animator>().SetTrigger("Attack");

            GameObject arrow = Instantiate<GameObject>(arrowPrefab);
            arrow.transform.position = this.gameObject.transform.GetChild(2).position;
            if (attackTarget!=null) {
                arrow.GetComponent<CannonBallController>().attackTarget = attackTarget;
                //cannonball.GetComponent<CannonBallController>().targetStats = attackTarget.GetComponent<CharacterStats>();
            }
            arrow.GetComponent<CannonBallController>().characterStats = characterStats;
            arrow.GetComponent<CannonBallController>().initialPosition = this.gameObject.transform.GetChild(2).position;
    }
}
