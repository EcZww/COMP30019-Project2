using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Fire : EnemyController
{
    [Header("Skill")]
    public int SkillCoolDown;
    private float coolDownLessTime;
    public Transform boomEffectTransform;
    public ParticleSystem boomEffectPrefab;
    private ParticleSystem boomEffect;
    public ParticleSystem burningPrefab;
    private ParticleSystem burningEffect;
    private bool burning = false;
    private GameObject currentTarget;
    private float burningEffectLessTime = 0;


    void Start() {
        coolDownLessTime = 0;
    }

    new void Update() {
        base.Update();
        Burning();
        if (burning && attackTarget != null) BurningEffect();
    }

    public void Burning() {
        if (attackTarget != null && coolDownLessTime<0) {
            animator.SetTrigger("Skill");
            currentTarget = attackTarget;
            coolDownLessTime = SkillCoolDown;
        } else {
            coolDownLessTime -= Time.deltaTime;
        }
    }

    public void PlaySkill() {
        boomEffect = Instantiate<ParticleSystem>(boomEffectPrefab);
        boomEffect.transform.position = boomEffectTransform.position;
        boomEffect.Play();
        StartCoroutine(CountBurning());
    }

    IEnumerator CountBurning()
    {
        currentTarget.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
        burning = true;
        yield return new WaitForSeconds(5f);
        currentTarget.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        burning = false;
    }

    public void BurningEffect() {
        if (burningEffectLessTime<=0) {
            burningEffect = Instantiate<ParticleSystem>(burningPrefab);
            burningEffect.transform.position = currentTarget.transform.position;
            burningEffect.Play();
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.GetDamage(characterStats, targetStats);
            burningEffectLessTime = 1f;
        } else {
            burningEffectLessTime -= Time.deltaTime;
        }
    }
}
