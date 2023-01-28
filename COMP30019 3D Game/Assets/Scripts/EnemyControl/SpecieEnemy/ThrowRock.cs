using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ThrowRock : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Basic Settings")]
    public float force;
    public ParticleSystem boomEffectPrefab;
    private ParticleSystem boomEffect;
    private Vector3 direction;
    [HideInInspector]
    public GameObject target;
    [HideInInspector]
    public CharacterStats characterStats;
    [HideInInspector]
    public CharacterStats targetStats;

    void Start() {
        rb = GetComponent<Rigidbody>();
        targetStats = target.GetComponent<CharacterStats>();
        FlyToTarget();
    }

    public void FlyToTarget() {
        if (target == null) target = FindObjectOfType<PlayerController>().gameObject;
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
        StartCoroutine(WaitForDestroy());
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player")) {
            if (other.gameObject.GetComponent<NavMeshAgent>().enabled) other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
            boomEffect = Instantiate<ParticleSystem>(boomEffectPrefab);
            boomEffect.transform.position = target.transform.position;
            other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force * 20;
            other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.GetDamage(characterStats, targetStats);
            boomEffect.Play();
        }  
    }

    IEnumerator WaitForDestroy() {
        yield return new WaitForSeconds(5f);
        Destroy(this.gameObject);
    }
}
