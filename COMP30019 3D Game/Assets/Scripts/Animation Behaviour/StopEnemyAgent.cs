using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class StopEnemyAgent : StateMachineBehaviour
{
    Vector3 position;
    Quaternion rotation;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.GetComponent<NavMeshAgent>().isStopped = true;
        animator.GetComponent<NavMeshAgent>().enabled = false;
        //position = animator.GetComponent<NavMeshAgent>().transform.localPosition;
        //rotation = animator.GetComponent<NavMeshAgent>().transform.rotation;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.GetComponent<NavMeshAgent>().isStopped = true;
        //animator.GetComponent<NavMeshAgent>().transform.localPosition = position;
        //animator.GetComponent<NavMeshAgent>().transform.rotation = rotation;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.GetComponent<NavMeshAgent>().isStopped = false;
        animator.GetComponent<NavMeshAgent>().enabled = true;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
