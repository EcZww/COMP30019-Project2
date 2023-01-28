using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce = 0.1f;
    public void KickOff()
    {
        if(attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            //attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.transform.position = attackTarget.transform.position + direction * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
