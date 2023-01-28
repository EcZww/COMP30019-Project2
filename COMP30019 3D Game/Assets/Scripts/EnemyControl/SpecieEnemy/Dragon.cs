using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Dragon : EnemyController
{
    protected override void GetNewCheckPosition() 
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange) + 400f;
        float randomZ = Random.Range(-patrolRange, patrolRange) + 400f;
        Vector3 randomPoint = new Vector3(initialPosition.x + randomX, transform.position.y, initialPosition.z + randomZ);
        NavMeshHit hit;
        checkPosition = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }
}