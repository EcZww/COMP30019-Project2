using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Text;

public enum EnemyState {GUARD, PATROL, CHASE, DEAD}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour
{   
    protected NavMeshAgent agent;
    private EnemyState enemyState;
    protected Animator animator;
    [HideInInspector]
    public CharacterStats characterStats;

    [Header("Basic Setttings")]
    public float sightRadius = 12f;
    public float lookAtTime;
    public bool isGuard = false;
    protected GameObject attackTarget;
    private float speed = 2f;
    private bool isWalk = true;
    private bool isChase = false;
    private bool isFollow = false;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public bool isFreezed = false;
    [HideInInspector]
    public bool isBurning = false;
    private float beBurningEffectLessTime = 0;
    [HideInInspector]
    public GameObject attacker;
    protected Vector3 initialPosition;
    private float lastAttackTime;
    private bool playerDead;
    private Quaternion initialRotation;
    public ParticleSystem deathParticleSystem;
    private ParticleSystem dieEffect;
    public ParticleSystem beBurningPrefab;

    [Header("Patrol State")]
    public float patrolRange;
    protected float remainLookAtTime;
    protected Vector3 checkPosition;
    private bool hasReturnOriginPosition = false;
    private bool hasInitializedDieing = false;
    private float slowlyRestoreHealthCooldown = 0.1f;
    private float slowlyRestoreHealthCooldownLess = 0f;
    [Header("Audio Settings")]
    private AudioSource audioSource;
    public AudioClip onHitSound;



    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        speed = agent.speed;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        characterStats = GetComponent<CharacterStats>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start() {   
        if (isGuard) {
            enemyState = EnemyState.GUARD;
        } else {
            enemyState = EnemyState.PATROL;
            GetNewCheckPosition();
        }
        GameManager.Instance.enemies.Add(this.gameObject);
        
    }

    void OnEnable() {

    }

    void OnDisable() {   
        //GameManager.Instance.enemies.Remove(this.gameObject);
        if (!GameManager.IsInitialized) return;
        if (GetComponent<LootBonus>() && isDead) GetComponent<LootBonus>().Spawnloot();
    }

    void OnDestroy() {
        GameManager.Instance.enemies.Remove(this.gameObject);
    }

    // Update is called once per frame
    protected void Update() {
        // Return to Last position that stored
        if (!hasReturnOriginPosition) {
            if (characterStats.CharacterPosition != Vector3.zero && characterStats.CharacterRotation != new Quaternion(0,0,0,0)) {
                transform.position = characterStats.CharacterPosition;
                transform.rotation = characterStats.CharacterRotation;
                // Show the enemies health bar that has injuries before
                if (characterStats.CurrentHealth < characterStats.MaxHealth) {
                    characterStats.refreshHealthBar();
                } else {
                    LevelUp();
                    characterStats.CurrentHealth = characterStats.MaxHealth;
                    
                }
                hasReturnOriginPosition = true;
            }
        }

        // keep the enemies be dieing that were dead before storing data
        if (!hasInitializedDieing) {
            if (characterStats.characterData.isDieing == true) {
                GameManager.Instance.DieAndRespawn(this.gameObject);
                this.gameObject.SetActive(false);
            }
            hasInitializedDieing = true;
        }

        isDead = characterStats.CurrentHealth == 0;
        if (!playerDead) {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;  
        }
        if (isBurning) BeBurningEffect();
        LevelUp();
    }

    private void SwitchAnimation()
    {
        animator.SetBool("Walk", isWalk);
        animator.SetBool("Chase", isChase);
        animator.SetBool("Follow", isFollow);
        animator.SetBool("Critical", characterStats.isCritical);
        animator.SetBool("Dead", isDead);
    }

    private void SwitchStates()
    {
        // found the enemy, chase the enemy
        if (isDead) enemyState = EnemyState.DEAD;
        else if(FoundFlayer()) enemyState = EnemyState.CHASE;
        
        switch(enemyState)
        {
            case EnemyState.GUARD:
                agent.speed = speed;
                isChase = false;
                SlowlyRestore();
                if (transform.position != initialPosition) {
                    isWalk = true;
                    if (agent.enabled) {
                        agent.isStopped = false;
                        agent.destination = initialPosition;
                    }
                    if (Vector3.SqrMagnitude(initialPosition - transform.position) <= agent.stoppingDistance) {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, 0.05f);
                    }
                }
                break;
            case EnemyState.PATROL:
                isChase = false;
                agent.speed = speed;
                SlowlyRestore();
                // determine whether the enemy have arrived the check position
                if (Vector3.Distance(checkPosition, transform.position) <= agent.stoppingDistance) {
                    isWalk = false;
                    if (remainLookAtTime > 0) remainLookAtTime -= Time.deltaTime;
                    else GetNewCheckPosition();
                } else {
                    isWalk = true;
                    if (agent.enabled != false) agent.destination = checkPosition;
                }
                break;
            case EnemyState.CHASE:
                agent.speed = speed * 2;
                isWalk = false;
                isChase = true;
                characterStats.refreshHealthBar();
                // chase the PlayerSettings
                if (FoundFlayer() && agent.enabled) {
                    //Debug.Log("Chasing!");
                    agent.destination = attackTarget.transform.position;
                    isFollow = true;
                    agent.isStopped = false;
                }
                // Return to last state
                else {
                    isFollow = false;
                    if (remainLookAtTime > 0 && agent.enabled) {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    } else if (isGuard) enemyState = EnemyState.GUARD;
                    else enemyState = EnemyState.PATROL;
                }
                //attack player when player in the attack range
                if (TargetInAttackRange() || TargetInSkillRange()) {
                    isFollow = false;
                    if (agent.enabled) agent.isStopped = true;
                    if (lastAttackTime < 0) {
                        lastAttackTime = characterStats.attackData.coolDown;
                        // critical attack determine
                        characterStats.isCritical = Random.value <= characterStats.attackData.criticalChance;
                        // take the attack action
                        Attack();
                    }
                }

                
                break;
            case EnemyState.DEAD:
                //agent.radius = 0;
                if (dieEffect == null) {
                    dieEffect = Instantiate<ParticleSystem>(deathParticleSystem);
                    dieEffect.transform.position = this.gameObject.transform.position;
                    dieEffect.Play();
                }
                if (characterStats.characterData.isBoss) {
                    Destroy(this.gameObject, 3f);
                    break;
                }
                StartCoroutine(Die());
                GameManager.Instance.DieAndRespawn(this.gameObject);
                // Set tutorial active if defeat the initial slime
                if (this.gameObject.name == "SlimeInitial") SceneController.Instance.slimeDead = true;
                if (this.gameObject.name == "TurtleShellInitial") SceneController.Instance.turtleDead = true;
                if (this.gameObject.name == "GruntInitial") SceneController.Instance.gruntDead = true;
                break;
        }
    }

    IEnumerator Die() {   
        yield return new WaitForSeconds(2.5f);
        characterStats.characterData.isDieing = true;
        GetComponent<HealthBarUI>().UIbar.gameObject.SetActive(false);
        characterStats.CurrentHealth = characterStats.MaxHealth;
        if (isGuard) {
            enemyState = EnemyState.GUARD;
        } else {
            enemyState = EnemyState.PATROL;
            GetNewCheckPosition();
        }
        //characterStats.refreshHealthBar();
        GetComponent<HealthBarUI>().healthSliderImage.fillAmount = 1;
        if (characterStats.characterData.isIce && attacker!= null) {
            attacker.GetComponent<NavMeshAgent>().enabled = true;
            attacker.GetComponent<Animator>().enabled = true;
            attacker.GetComponent<PlayerController>().isInputEnabled = true;
            attacker.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        }
        if (characterStats.characterData.isFire && attacker!= null) {
            attacker.transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        }
        this.gameObject.SetActive(false);
    }

    private bool FoundFlayer()
    {
        var collider = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var target in collider)
        {
            if (target.CompareTag("Player")) {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
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
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }

    //determine the player whether in the close range attack range
    protected bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    //determine the player whether in the long range attack range
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    protected virtual void Attack() 
    {
        if(!isFreezed) transform.LookAt(attackTarget.transform);
        // Close Range Attack animation
        if(TargetInSkillRange()) {
            animator.SetTrigger("Skill");
        }
        // Long Range Attack animation
        else if(TargetInAttackRange()) {
            animator.SetTrigger("Attack");
        }
    }

    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.GetDamage(characterStats, targetStats);
            targetStats.GetComponent<PlayerController>().PlayBeHitSound();
        }
    }

    public void playBeHitSound() {
        audioSource.PlayOneShot(onHitSound);
    }

    public void BeBurningEffect() {
        if (beBurningEffectLessTime<=0) {
            var burningEffect = Instantiate<ParticleSystem>(beBurningPrefab);
            burningEffect.transform.position = this.gameObject.transform.position;
            burningEffect.Play();
            if (this.enemyState != EnemyState.DEAD && characterStats.CurrentHealth>0) characterStats.GetDamage(attacker.GetComponent<PlayerController>().characterStats, characterStats);
            beBurningEffectLessTime = 1f;
        } else {
            beBurningEffectLessTime -= Time.deltaTime;
        }
    }

    private void SlowlyRestore() {
        if (slowlyRestoreHealthCooldownLess<0) {
            characterStats.SlowlyRestoreHealth();
            slowlyRestoreHealthCooldownLess = slowlyRestoreHealthCooldown;
        } else {
            slowlyRestoreHealthCooldownLess -= Time.deltaTime;
        }
    }

    public void LevelUp() {
        float levelBuff = 1.1f;
        int playerLevel = GameManager.Instance.playerStats.characterData.currentLevel-1;
        characterStats.MaxHealth = (int)(characterStats.templateCharacterData.maxHealth * Mathf.Pow(levelBuff, playerLevel));
        characterStats.BaseDefence = (int)(characterStats.templateCharacterData.baseDefence  * Mathf.Pow(levelBuff, playerLevel));
        characterStats.CurrentDefence = (int)(characterStats.templateCharacterData.currentDefence * Mathf.Pow(levelBuff, playerLevel));
        characterStats.attackData.baseMinDamage = (int)(characterStats.templateAttackData.baseMinDamage * Mathf.Pow(levelBuff, playerLevel));
        characterStats.attackData.baseMaxDamage = (int)(characterStats.templateAttackData.baseMaxDamage * Mathf.Pow(levelBuff, playerLevel));
        characterStats.attackData.minDamage = (int)(characterStats.templateAttackData.minDamage * Mathf.Pow(levelBuff, playerLevel));
        characterStats.attackData.maxDamage = (int)(characterStats.templateAttackData.maxDamage * Mathf.Pow(levelBuff, playerLevel));
        characterStats.characterData.killExperience = (int)(characterStats.templateCharacterData.killExperience * Mathf.Pow(levelBuff, playerLevel));
    }

}
