using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ice : EnemyController
{
    [Header("Skill")]
    public int SkillCoolDown;
    private float coolDownLessTime;
    public Transform boomEffectTransform;
    public ParticleSystem boomEffectPrefab;
    private ParticleSystem boomEffect;
    private GameObject currentTarget;
    private Vector3 playerPosition;
    private Quaternion playerRotation;
    private bool freezing = false;


    void Start() {
        coolDownLessTime = 0;
    }

    new void Update() {
        base.Update();
        Freeze();
    }

    public void Freeze() {
        if (attackTarget != null && coolDownLessTime<0) {
            animator.SetTrigger("Skill");
            currentTarget = attackTarget;
            coolDownLessTime = SkillCoolDown;
        } else {
            coolDownLessTime -= Time.deltaTime;
        }
        if (freezing) attackTarget.transform.rotation = playerRotation;
    }

    public void PlaySkill() {
        boomEffect = Instantiate<ParticleSystem>(boomEffectPrefab);
        boomEffect.transform.position = boomEffectTransform.position;
        boomEffect.Play();
        StartCoroutine(CountFreeze());
    }

    IEnumerator CountFreeze()
    {
        currentTarget.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 1);
        attackTarget.GetComponent<NavMeshAgent>().enabled = false;
        attackTarget.GetComponent<Animator>().enabled = false;
        attackTarget.GetComponent<PlayerController>().isInputEnabled = false;
        playerRotation = attackTarget.transform.rotation;
        freezing = true;
        yield return new WaitForSeconds(5f);
        attackTarget.GetComponent<NavMeshAgent>().enabled = true;
        attackTarget.GetComponent<Animator>().enabled = true;
        attackTarget.GetComponent<PlayerController>().isInputEnabled = true;
        freezing = false;
        currentTarget.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
    }
}
