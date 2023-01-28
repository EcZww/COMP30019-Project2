using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongRangeBallController : MonoBehaviour
{
    
    public float firingAngle = 45.0f;
    public float gravity = 9.8f;
    public float delayTime = 1f;
    public ParticleSystem boomEffectPrefab;
    public float waitTime = 0.5f;
    private ParticleSystem boomEffect;
    private Transform cannonBall;   
    [HideInInspector]
    public Vector3 initialPosition;
    [HideInInspector]
    public CharacterStats targetStats;
    [HideInInspector]
    public CharacterStats characterStats;
    [HideInInspector]
    public GameObject attackTarget;
    [HideInInspector]
    public Transform target; 
    public Vector3 forwardDirection;
    public float attackRange;  

    void Start() {
        if (attackTarget != null) target = attackTarget.transform;
        cannonBall = this.transform;
        if (attackTarget != null) targetStats = attackTarget.GetComponent<CharacterStats>();
    }

    void Update () {
        if (delayTime < 0) {
            if (Vector3.Distance(this.transform.position, initialPosition) > attackRange) Destroy(this.gameObject);
            if (target != null) SentCannonBall();
            else cannonBall.Translate(forwardDirection * Time.deltaTime * 15);
        } else {
            delayTime -= Time.deltaTime;
        }
	}

    // Handle collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            // Damage object
            other.gameObject.GetComponent<CharacterStats>().GetDamage(characterStats, other.gameObject.GetComponent<CharacterStats>());
            boomEffect = Instantiate<ParticleSystem>(boomEffectPrefab);
            boomEffect.transform.position = other.gameObject.transform.position;
            // Destroy self
            boomEffect.Play();
            Destroy(this.gameObject);
        }
    }

    void SentCannonBall()
    {
        // Calculate distance to target
        float target_Distance = Vector3.Distance(cannonBall.position, target.position);
 
        // Calculate the velocity needed to throw the object to the target at specified angle.
        float cannonBall_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);
 
        // Extract the X  Y component of the velocity
        float Vx = Mathf.Sqrt(cannonBall_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(cannonBall_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);
 
        // Calculate flight time.
        float flightDuration = target_Distance / Vx;
   
        // Rotate cannonBall to face the target.
        cannonBall.rotation = Quaternion.LookRotation(target.position - cannonBall.position);
       
        float elapse_time = 0;
 
        while (elapse_time < flightDuration)
        {
            cannonBall.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            elapse_time += Time.deltaTime;
            return;
        }
    }  
    
}