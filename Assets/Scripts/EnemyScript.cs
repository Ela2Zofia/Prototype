using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyScript : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private NavMeshAgent navMeshAgent;
    private Animator anim;
    private FOVScript fovScript;
    private float distance;
    private float range = 28.0f; // Range that a target can detect
    private float attackCd = 0.0f; // Instantiating attack cooldown variable
    public float followCd = -1.0f; // Instantiating follow (after sight of target lost) cooldown variable
    private bool isAttacking = false;
    private float damage = 8.0f; // Amount of damage dealt
    public float health = 100.0f; // Enemy's health value
    public float attackRange; // Enemy's attack range
    public float attackCdTime;

    public bool dead = false;

    public ParticleSystem muzzleEnemy;

    private AudioManager audioManager;

    Vector3 lookPosition;
    public int rotationSpeed = 3; //speed of turning

    void Awake() {
        target = GameObject.FindWithTag("Player").transform; //target the player
        audioManager = FindObjectOfType<AudioManager>();
    }
    void Start() {
        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        fovScript = gameObject.GetComponentInChildren<FOVScript>();

        if (navMeshAgent == null) {
            Debug.LogError("nav mesh agent attached to");
        } else {
            SetDestination();
        }
    }
    void Update () {
        // Stop idling enemy if: player can be seen, has recently been followed, or is a certain distance from the player (regardless of sight)
        distance = Vector3.Distance(transform.position, target.position);
        if (((distance <= range && inSight()) || followCd >= Time.time) && !dead) { 
            // Looking
            lookAtPlayer();
            // Movement
            if (pathReachable() && !isAttacking) {
                move();
            }
            // Attack
            if ((Vector3.Distance(transform.position, target.position) <= attackRange) && (Time.time >= attackCd) && (attackSight())) {
                int randomInt = Random.Range(0, 2);

                    switch (gameObject.tag) {
                        case("BasicEnemy"):
                            switch (randomInt) { // Randomly selects an attack animation to use
                                case(0):
                                    anim.SetBool("isPunch", true);
                                    break;
                                case(1):
                                    anim.SetBool("isKick", true);
                                    break;
                            }
                            break;
                        case("SoldierEnemy"):
                            anim.SetBool("isShoot", true);
                            break;
                    }

                isAttacking = true;
                navMeshAgent.isStopped = true;
                attackCd = Time.time + attackCdTime;
                }
            // Attacking
            if ((distance <= attackRange)) {
                startAttacking();
                if (Time.time >= attackCd) {
                    attack();
                }
            } else {
                stopAttacking();
            }
        } else {
            anim.SetBool("isActive", false);
            navMeshAgent.isStopped = true;
        }
    }

    // Animation event functions
    public void FinishKick()
    {
        anim.SetBool("isKick", false);
        anim.SetBool("isAttackIdle", true);
    }
    public void FinishPunch()
    {
        anim.SetBool("isPunch", false);
        anim.SetBool("isAttackIdle", true);
    }
    public void FinishShoot()
    {
        anim.SetBool("isShoot", false);
        anim.SetBool("isShootIdle", true);
    }
    public void DealDamage()
    {
         switch (gameObject.tag) {
            case("BasicEnemy"):
                audioManager.Play("EnemyHitSound");
                target.GetComponent<PlayerHealth>().health = target.GetComponent<PlayerHealth>().health - damage;
                break;
            case("SoldierEnemy"):
                int random;
                audioManager.Play("EnemyGunshotSound");
                muzzleEnemy.Play();
                if (distance < attackRange*0.25f) {
                    random = 0;
                } else if (distance < attackRange*0.4f) {
                    random = Random.Range(0, 1);
                } else if (distance < attackRange*0.8f) {
                    random = Random.Range(0, 2);
                } else {
                    random = Random.Range(0, 3);
                }
                if (random == 0){
                    audioManager.Play("BulletImpactPlayer");
                    target.GetComponent<PlayerHealth>().health = target.GetComponent<PlayerHealth>().health - damage;
                }
                break;
        }
    }

    // Nav mesh agent path finding
    private void SetDestination()
    {
        if (transform != null)
        {
            Vector3 targetVector = target.position;
            navMeshAgent.SetDestination(targetVector);
        }
    }
    private void lookAtPlayer()
    {
        lookPosition = target.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation,
        Quaternion.LookRotation(lookPosition), rotationSpeed*Time.deltaTime);
    }
    private void move()
    {
        anim.SetBool("isActive", true);
        navMeshAgent.isStopped = false;
        SetDestination();
    }
    private void attack()
    {
        int randomInt = Random.Range(0, 2);
        switch (gameObject.tag) {
            case("BasicEnemy"):
                switch (randomInt) { // Randomly selects an attack animation to use
                    case(0):
                        anim.SetBool("isPunch", true);
                        break;
                    case(1):
                        anim.SetBool("isKick", true);
                        break;
                }
                break;
            case("SoldierEnemy"):
                    anim.SetBool("isShoot", true);
                    break;
        }
        attackCd = Time.time + attackCdTime;
    }
    private void startAttacking() {
        navMeshAgent.isStopped = true;
        isAttacking = true;
        anim.SetBool("isAttacking", true);
    }
    private void stopAttacking() {
        isAttacking = false;
        anim.SetBool("isAttacking", false);
        switch (gameObject.tag) {
            case("BasicEnemy"):
                anim.SetBool("isPunch", false);
                anim.SetBool("isKick", false);
                break;
            case("SoldierEnemy"):
                anim.SetBool("isShoot", false);
                break;
        }
        navMeshAgent.isStopped = false;
    }
    // Checks if player is in enemy's field of vision
    private bool inSight()
    {
        bool isInSight = fovScript.inSight(target);
        if (isInSight) {
            followCd = Time.time + 7.0f;
        }
        return fovScript.inSight(target);
    }

    // Checks if enemy is turned towards the player enough to attack
    private bool attackSight()
    {
        float dot = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
        if(dot > 0.75f) {
            return true;
        }
        return false;
    }

    // Returns whether a path is reachable by the agent
    private bool pathReachable()
    {
        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(target.position, path);
        if (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathInvalid) {
            return true;
        }
        return false;
    }


    public void startDestroyEnemy() {
        dead = true;
        anim.SetBool("isDead", true);
    }

    public void finishDestroyEnemy() {
        Destroy(gameObject);
    }
}
