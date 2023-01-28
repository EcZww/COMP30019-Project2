using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    
    public float firingAngle = 45.0f;
    public float gravity = 9.8f;
    public float delayTime = 5f;
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

    void Start() {
        if (attackTarget == null) Destroy(this.gameObject);
        target = attackTarget.transform;
        cannonBall = this.transform;
        targetStats = attackTarget.GetComponent<CharacterStats>();
    }

    void Update () {
        if (delayTime < 0) {
            if (Vector3.Distance(this.transform.position, initialPosition) > 200) Destroy(this.gameObject);
            SentCannonBall();
        } else {
            delayTime -= Time.deltaTime;
        }
	}

    // Handle collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // Damage object
            targetStats.GetDamage(characterStats, targetStats);
            boomEffect = Instantiate<ParticleSystem>(boomEffectPrefab);
            boomEffect.transform.position = target.position;
            
            // Destroy self
            StartCoroutine(WaitTheCannonBall());
        }
    }

    IEnumerator WaitTheCannonBall()
    {
        //yield on a new YieldInstruction that waits for 0.5 seconds.
        yield return new WaitForSeconds(waitTime);
        boomEffect.Play();
        Destroy(this.gameObject);
    }

    void SentCannonBall()
    {
        // Calculate distance to target
        if (target.position == null) Destroy(this.gameObject);
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