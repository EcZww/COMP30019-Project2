using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    public new Camera camera;
    public float speed = 1;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public GameObject LongRangeBallPrefab;
    public Transform longRangeFirePosition;
    public float lockEnemyRadius;
    private Animator animator; 
    private Vector3 movement;
    NavMeshAgent agent;
    [HideInInspector]
    public CharacterStats characterStats;
    private float lastAttackTime;
    private bool comboWindow;
    private bool isDead = false;
    private List<GameObject> enemyTargets = new List<GameObject>();
    [HideInInspector]
    public bool isInputEnabled = true;
    [Header("FreezeSkill")]
    public int freezeSkillRange;
    public int freezeSkillTime;
    public int freezeSkillCooldown;
    private float freezeSkillCooldownLess = 0;
    public Cooldown freezeSkillUI;
    [Header("ImmunitySkill")]
    public int immunitySkillTime;
    public int immunitySkillCooldown;
    private float immunitySkillCooldownLess = 0;
    public Cooldown immunitySkillUI;
    [Header("RestoreHealthSkill")]
    public int restoreHealthSkillTime;
    public int restoreHealthSkillCooldown;
    private float restoreHealthSkillCooldownLess = 0;
    private float restoringEffectLessTime;
    public ParticleSystem restoreParticlePrefab;
    private bool isRestoringHealth = false;
    public Cooldown restoreHealthSkillUI;
    [Header("BurningSkill")]
    public int burningSkillRange;
    public int burningSkillTime;
    public int burningSkillCooldown;
    private float burningSkillCooldownLess = 0;
    public Cooldown burningSkillUI;
    [Header("AudioSettings")]
    private AudioSource audioSource;
    public AudioClip unarmedSound;
    public AudioClip cirtUnarmedSound;
    public AudioClip oneHandSound;
    public AudioClip critOneHandSound;
    public AudioClip twoHandSound;
    public AudioClip stoneSound;
    public AudioClip fireSound;
    public AudioClip iceSound;
    public AudioClip treeSound;
    public AudioClip rangeSound;
    public AudioClip beHitSound;
    public AudioClip footstepSound;


    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable() {
        GameManager.Instance.RigisterPlayer(characterStats);
        //if (characterStats.characterTransform != null) this.transform.position = characterStats.characterTransform.position;
        if (SceneManager.GetActiveScene().name == "MenuScene" || SceneManager.GetActiveScene().name == "OpeningScene") {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Start() {
        // get player data
        SaveDataManager.Instance.LoadPlayerData();

        freezeSkillUI.cooldownTime = freezeSkillCooldown;
        immunitySkillUI.cooldownTime = immunitySkillCooldown;
        restoreHealthSkillUI.cooldownTime = restoreHealthSkillCooldown;
        burningSkillUI.cooldownTime = burningSkillCooldown;
    }

    void Update() {
        isDead = characterStats.CurrentHealth<=0;
        if (isDead) SaveDataManager.Instance.playerDead = true;
        MoveControl();
        SwitchAnimator();
        lastAttackTime -= Time.deltaTime;
        #region Skill CoolDown Logic
        freezeSkillCooldownLess -= Time.deltaTime;
        if (characterStats.hasFreezeSkill) freezeSkillUI.hasSkill = true;
        freezeSkillUI.cooldownTimeLess = freezeSkillCooldownLess;
        immunitySkillCooldownLess -= Time.deltaTime;
        if (characterStats.hasImmunitySkill) immunitySkillUI.hasSkill = true;
        immunitySkillUI.cooldownTimeLess = immunitySkillCooldownLess;
        restoreHealthSkillCooldownLess -= Time.deltaTime;
        if (characterStats.hasRestoreHealthSkill) restoreHealthSkillUI.hasSkill = true;
        restoreHealthSkillUI.cooldownTimeLess = restoreHealthSkillCooldownLess;
        burningSkillCooldownLess -= Time.deltaTime;
        if (characterStats.hasBurningSkill) burningSkillUI.hasSkill = true;
        burningSkillUI.cooldownTimeLess = burningSkillCooldownLess;
        if (isRestoringHealth) RestoringEffect();
        #endregion
        characterStats.CharacterPosition = transform.position;
        characterStats.CharacterRotation = transform.rotation;
    }

    IEnumerator waitToMenu() {
        yield return new WaitForSeconds(3f);
        SaveDataManager.Instance.playerDead = true;
    }

    private void SwitchAnimator() {
        animator.SetFloat("Speed", agent.speed);
        animator.SetBool("Dead", isDead);
    }

#region Player_Keyboard_Controller

    void MoveControl() {
        if (!InventoryManager.Instance.isOpen) mouseDown();
        if (isInputEnabled) keyBoardPress();
    }

    private void keyBoardPress() {
        movement = new Vector3(0, 0, 0);

        if (isDead || InventoryManager.Instance.isOpen) return;
        
        if (Input.GetKey(KeyCode.W))
        {
            movement += camera.transform.forward * Time.deltaTime *  speed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            movement += -camera.transform.forward * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            movement += -camera.transform.right * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movement += camera.transform.right * Time.deltaTime * speed;
        }
        #region Player_Skill_Key
        if (Input.GetKey(KeyCode.Q) && characterStats.hasFreezeSkill)
        {
            if(freezeSkillCooldownLess<0) {
                IceSkill();
                freezeSkillCooldownLess = freezeSkillCooldown;
            }
        }

        if (Input.GetKey(KeyCode.E) && characterStats.hasBurningSkill)
        {
            if(burningSkillCooldownLess<0) {
                FireSkill();
                burningSkillCooldownLess = burningSkillCooldown;
            }
        }

        if (Input.GetKey(KeyCode.R) && characterStats.hasImmunitySkill)
        {
            if(immunitySkillCooldownLess<0) {
                ImmunitySkill();
                immunitySkillCooldownLess = immunitySkillCooldown;
            }
        }

        if (Input.GetKey(KeyCode.T) && characterStats.hasRestoreHealthSkill)
        {
            if(restoreHealthSkillCooldownLess<0) {
                RestoreHealthSkill();
                restoreHealthSkillCooldownLess = restoreHealthSkillCooldown;
            }
        }

        #endregion

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetTrigger("RunOver");
        } 
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            agent.speed = runSpeed;
            movement *= runSpeed;
        } 
        else{
            agent.speed = walkSpeed;
            movement *= walkSpeed;
        }

        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("DiveRoll"))
        {
            
            agent.speed = runSpeed*1.25f;
            movement *= 1.25f;
        }
        if (movement == Vector3.zero) agent.speed = 0;
        transform.LookAt(movement + transform.position);
        if (agent.enabled) agent.Move(movement);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Jump");
        } 


        return;
    }
#endregion

#region Player_Attack_Logic
    private void mouseDown() {
        if (Input.GetMouseButtonDown(0) && (lastAttackTime <= 0)) {
            lastAttackTime = characterStats.attackData.coolDown;
            // determine critical state
            characterStats.isCritical = Random.value <= characterStats.attackData.criticalChance;
            animator.SetBool("Critical", characterStats.isCritical);
            // Player Lock the enemy when the player is close to enemy
            Transform enemyTransform = FoundEnemy();
            if (enemyTransform != null) {
                this.transform.LookAt(enemyTransform, Vector3.up);
            } else {
                this.transform.LookAt(camera.transform, Vector3.up);
                this.transform.RotateAround(this.transform.position, Vector3.up, 180);
            }
            // let player play the attack animation
            //if (!isDead) Attack();
            if (!isDead) animator.SetTrigger("Attack");
        }
    }
    private Transform FoundEnemy() {
        enemyTargets.Clear();
        var collider = Physics.OverlapSphere(transform.position, characterStats.attackData.attackRange);
        foreach (var target in collider)
        {
            if (target.CompareTag("Enemy") && target.gameObject.GetComponent<CharacterStats>().characterData.currentHealth>0) {               
                enemyTargets.Add(target.gameObject);
            }
        }
        if (enemyTargets.Count != 0) return enemyTargets[0].transform;
        else return null;
    }

    // show the near enemies health bar that their health is not ful
    

    void HitCloseRange()
    {
        if (enemyTargets.Count != 0) {
            foreach (GameObject attackTarget in enemyTargets) {
                var targetStats = attackTarget.GetComponent<CharacterStats>();
                targetStats.GetDamage(characterStats, targetStats);
                attackTarget.GetComponent<EnemyController>().playBeHitSound();
                attackTarget.GetComponent<EnemyController>().attacker = this.gameObject;
            }     
        }
    }

    void HitLongRange()
    {
        GameObject projectile = Instantiate<GameObject>(LongRangeBallPrefab);
        projectile.transform.position = longRangeFirePosition.position;
        if (enemyTargets.Count != 0) {
            GameObject attackTarget = enemyTargets[enemyTargets.Count-1];
            if (attackTarget!=null) {        
                projectile.GetComponent<LongRangeBallController>().attackTarget = attackTarget;
                attackTarget.GetComponent<EnemyController>().attacker = this.gameObject;
            }
        }
        projectile.GetComponent<LongRangeBallController>().forwardDirection = camera.transform.forward;
        projectile.GetComponent<LongRangeBallController>().characterStats = characterStats;
        projectile.GetComponent<LongRangeBallController>().initialPosition = this.gameObject.transform.GetChild(2).position;
        projectile.GetComponent<LongRangeBallController>().attackRange = characterStats.attackData.attackRange;
    }
#endregion

#region Player_Skill_Logic
    private void IceSkill() {
        var collider = Physics.OverlapSphere(transform.position, freezeSkillRange);
        foreach (var target in collider)
        {
            if (target.CompareTag("Enemy") && target.gameObject.GetComponent<CharacterStats>().characterData.currentHealth>0) {               
                StartCoroutine(Freeze(target));
            }
        }
        audioSource.PlayOneShot(iceSound); 
    }

    IEnumerator Freeze(Collider target)
    {
        target.GetComponentInChildren<Renderer>().material.color = new Color(0, 0, 1, 1);
        target.GetComponent<NavMeshAgent>().enabled = false;
        target.GetComponent<Animator>().enabled = false;
        target.gameObject.GetComponent<EnemyController>().isFreezed = true;
        yield return new WaitForSeconds(freezeSkillTime);
        target.GetComponentInChildren<Renderer>().material.color = new Color(1, 1, 1, 1);
        target.GetComponent<NavMeshAgent>().enabled = true;
        target.GetComponent<Animator>().enabled = true;
        target.gameObject.GetComponent<EnemyController>().isFreezed = false;
    }

    private void ImmunitySkill() {
        StartCoroutine(Immunity());
        audioSource.PlayOneShot(stoneSound);
    }
    IEnumerator Immunity()
    {
        this.GetComponentInChildren<Renderer>().material.color = new Color(3, 3, 0, 1);
        this.characterStats.isReturnDamageAndImmunity = true;
        yield return new WaitForSeconds(immunitySkillTime);
        this.GetComponentInChildren<Renderer>().material.color = new Color(1, 1, 1, 1);
        this.characterStats.isReturnDamageAndImmunity = false;
    }

    private void RestoreHealthSkill() {
        StartCoroutine(RestoreHealth());
        audioSource.PlayOneShot(treeSound);
    }

    IEnumerator RestoreHealth()
    {
        this.transform.GetChild(1).GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
        isRestoringHealth = true;
        yield return new WaitForSeconds(restoreHealthSkillTime);
        this.transform.GetChild(1).GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
        isRestoringHealth = false;
    }
    public void RestoringEffect() {
        this.characterStats.RestoreHealthSkill();
        if (restoringEffectLessTime<=0) {
            var burningEffect = Instantiate<ParticleSystem>(restoreParticlePrefab);
            burningEffect.transform.position = this.gameObject.transform.position;
            burningEffect.Play();
            restoringEffectLessTime = 1f;
        } else {
            restoringEffectLessTime -= Time.deltaTime;
        }
    }

    private void FireSkill() {
        var collider = Physics.OverlapSphere(transform.position, burningSkillRange);
        audioSource.PlayOneShot(fireSound);
        foreach (var target in collider)
        {
            if (target.CompareTag("Enemy") && target.gameObject.GetComponent<CharacterStats>().characterData.currentHealth>0) {          
                StartCoroutine(Burning(target));
            }
        } 
    }

    IEnumerator Burning(Collider target)
    {
        target.GetComponentInChildren<Renderer>().material.color = new Color(1, 0, 0, 1);
        target.gameObject.GetComponent<EnemyController>().isBurning = true;
        target.gameObject.GetComponent<EnemyController>().attacker = this.gameObject;
        yield return new WaitForSeconds(burningSkillTime);
        target.GetComponentInChildren<Renderer>().material.color = new Color(1, 1, 1, 1);
        target.gameObject.GetComponent<EnemyController>().isBurning = false;
    }

#endregion

#region Play_Sound_Logic
    private void PlayUnarmedSound()
    {
        audioSource.PlayOneShot(unarmedSound, Random.Range(0.75f,1));
    }
    private void PlayCritUnarmedSound()
    {
        audioSource.PlayOneShot(cirtUnarmedSound, Random.Range(0.75f,1));
    }
    private void PlayOneHandSound()
    {
        audioSource.PlayOneShot(oneHandSound, Random.Range(0.75f,1));
    }
    private void PlayCritOneHandSound()
    {
        audioSource.PlayOneShot(critOneHandSound, Random.Range(0.75f,1));
    }
    private void PlayTwoHandSound()
    {
        audioSource.PlayOneShot(twoHandSound, Random.Range(0.75f,1));
    }
    private void PlayFootstepSound()
    {
        audioSource.PlayOneShot(footstepSound, Random.Range(0.75f,1));
    }

    public void PlayBeHitSound() {
        audioSource.PlayOneShot(beHitSound, Random.Range(0.75f,1));
    }
#endregion
}
