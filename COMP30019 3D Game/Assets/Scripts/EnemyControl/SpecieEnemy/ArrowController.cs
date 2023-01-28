using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [HideInInspector]
    public Vector3 direction = new Vector3(0,1,0);

    [HideInInspector]
    public float arrowSpeed;
    public Vector3 initialPosition;
    public CharacterStats targetStats;
    public CharacterStats characterStats;

    // Update is called once per frame
    void Update () {
        this.transform.Translate(direction * Time.deltaTime * arrowSpeed);
        if (Vector3.Distance(this.transform.position, initialPosition) > 40) Destroy(this.gameObject);
	}

    // Handle collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // Damage object
            targetStats.GetDamage(characterStats, targetStats);

            // Destroy self
            Destroy(this.gameObject);
        }
    }
}
