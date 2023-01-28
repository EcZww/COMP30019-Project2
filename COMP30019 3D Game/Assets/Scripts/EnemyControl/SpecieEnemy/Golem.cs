using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public int kickOffCoolDown;
    public float kickForce = 10f;
    public int ImmunityCoolDown;
    private float coolDownLessTime;
    public int ImmunityDuration;
    private float ImmunityLessTime;
    public GameObject rockPrefab;
    public Transform rockInitialPosition;
    private Color originalColor;

    void Start() {
        coolDownLessTime = 0;
        ImmunityLessTime = ImmunityDuration;
        originalColor = this.gameObject.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color;
    }

    new void Update() {
        base.Update();
        Immunity();
    }

    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {   
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            if (targetStats.GetComponent<NavMeshAgent>().enabled) targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.transform.position = attackTarget.transform.position + direction * kickForce;
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.GetDamage(characterStats, targetStats);
        }
    }

    public void ThrowRock() {
        if(attackTarget != null) {
            var rock = Instantiate(rockPrefab, rockInitialPosition.position, Quaternion.identity);
            rock.GetComponent<ThrowRock>().target = attackTarget;
            rock.GetComponent<ThrowRock>().characterStats = characterStats;
        }
    }

    public void Immunity() {
        if (attackTarget != null && coolDownLessTime<0) {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            this.gameObject.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(3, 3, 0, 1);
            ImmunityLessTime -= Time.deltaTime;
            agent.enabled = true;
            agent.isStopped = true;
            animator.enabled = false;
            characterStats.isReturnDamageAndImmunity = true;
            if (ImmunityLessTime < 0) {
                coolDownLessTime = ImmunityCoolDown;
                ImmunityLessTime = ImmunityDuration;
                this.gameObject.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = originalColor;
                agent.enabled = true;
                agent.isStopped = false;
                animator.enabled = true;
                characterStats.isReturnDamageAndImmunity = false;
            }
        } else {
            coolDownLessTime -= Time.deltaTime;
        }
    }
}
