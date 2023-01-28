using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DragonForView : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    public float lookAtTime;
    private float speed = 2f;
    private bool isWalk = true;
    protected Vector3 initialPosition;
    private Quaternion initialRotation;
    public float patrolRange;
    protected float remainLookAtTime;
    protected Vector3 checkPosition;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        speed = agent.speed;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        GetNewCheckPosition();
    }

    // Update is called once per frame
    void Update()
    {
        agent.speed = speed;
        // determine whether the enemy have arrived the check position
        if (Vector3.Distance(checkPosition, transform.position) <= agent.stoppingDistance+1) {
            isWalk = false;
            if (remainLookAtTime > 0) remainLookAtTime -= Time.deltaTime;
            else GetNewCheckPosition();
        } else {
            isWalk = true;
            agent.destination = checkPosition;
        }
        animator.SetBool("Walk", isWalk);
    }

    protected virtual void GetNewCheckPosition()
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(initialPosition.x + randomX, transform.position.y, initialPosition.z + randomZ);
        NavMeshHit hit;
        checkPosition = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }
}


