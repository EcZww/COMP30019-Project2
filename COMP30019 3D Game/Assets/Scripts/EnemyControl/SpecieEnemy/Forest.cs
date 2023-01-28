using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Forest : EnemyController
{
    [Header("Skill")]
    public int SkillCoolDown;
    private float coolDownLessTime;
    public int SkillDuration;
    private float SkillLessTime;
    public Material originalMaterial;
    public Material skillMaterial;
    public ParticleSystem boomEffectPrefab;
    private ParticleSystem boomEffect;

    void Start() {
        coolDownLessTime = 15;
        SkillLessTime = SkillDuration;
    }

    new void Update() {
        base.Update();
        RestoreHealth();
    }

    public void RestoreHealth() {
        if (attackTarget != null && coolDownLessTime<0) {
            this.gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = skillMaterial;
            SkillLessTime -= Time.deltaTime;
            agent.enabled = true;
            agent.isStopped = true;
            animator.SetTrigger("Skill");
            characterStats.RestoreHealthSkill();
            boomEffect = Instantiate<ParticleSystem>(boomEffectPrefab);
            boomEffect.transform.position = this.gameObject.transform.position;
            boomEffect.Play();
            this.gameObject.GetComponent<HealthBarUI>().UpdateHealthBar(characterStats.CurrentHealth, characterStats.MaxHealth);
            if (SkillLessTime < 0) {
                coolDownLessTime = SkillCoolDown;
                SkillLessTime = SkillDuration;
                this.gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = originalMaterial;
                agent.enabled = true;
                agent.isStopped = false;
            }
        } else {
            coolDownLessTime -= Time.deltaTime;
        }
    }
}
