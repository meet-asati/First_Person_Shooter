using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public int health = 100;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public float bloom;
    public float fireRate;
    private float lastShotTime = 0f;

    public Material hitMat;

    private Rigidbody rb;
    private Renderer rend;
    private Material originalMaterial;

    // AI Settings
    public float currentPointIndex;
    public Vector3 currentTarget;
    public float positionThreshold;
    public float idleTime = 5f;
    public float attackDistance = 5f;
    public float maxVisionDistance = 20f;
    public float minChasingHealth = 30f;

    public Transform[] patrolPoints;
    private float idleTimeCounter;
    private Transform playerTranform;
    private bool canSeePlayer;
    private Vector3 lastKnownPlayerPosition;

    private NavMeshAgent agent;

    public enum State
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking
    }

    public State state { get; private set; }

    private EnemyState currentState;

    public AudioClip shootingSFX;

    private abstract class EnemyState
    {
        protected Enemy enemy;

        public void SetContext(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public abstract void Update();
    }

    private sealed class IdleState : EnemyState
    {
        public override void Enter()
        {
            enemy.agent.ResetPath();
        }

        public override void Update()
        {
            enemy.idleTimeCounter -= Time.deltaTime;

            if (enemy.idleTimeCounter < 0f)
            {
                enemy.idleTimeCounter = enemy.idleTime;
                enemy.TransitionTo(
                    new PatrollingState(),
                    State.Patrolling);
            }
        }
    }

    private sealed class PatrollingState : EnemyState
    {
        public override void Update()
        {
            if (Vector3.Distance(
                    enemy.currentTarget,
                    enemy.transform.position) < enemy.positionThreshold)
            {
                float chance = Random.Range(0f, 100f);

                if (chance < 10f)
                {
                    enemy.TransitionTo(
                        new IdleState(),
                        State.Idle);

                    return;
                }

                enemy.currentPointIndex++;

                enemy.currentTarget =
                    enemy.patrolPoints[
                        (int)(enemy.currentPointIndex %
                              enemy.patrolPoints.Length)].position;
            }
            else
            {
                enemy.agent.SetDestination(enemy.currentTarget);
            }
        }
    }

    private sealed class ChasingState : EnemyState
    {
        public override void Enter()
        {
            enemy.idleTimeCounter = enemy.idleTime;
        }

        public override void Update()
        {
            enemy.agent.SetDestination(enemy.lastKnownPlayerPosition);

            float distanceToPlayer = Vector3.Distance(
                enemy.transform.position,
                enemy.playerTranform.position);

            if (enemy.health < enemy.minChasingHealth)
            {
                enemy.TransitionTo(
                    new PatrollingState(),
                    State.Patrolling);
            }
            else if (distanceToPlayer <= enemy.attackDistance &&
                     enemy.canSeePlayer)
            {
                enemy.TransitionTo(
                    new AttackingState(),
                    State.Attacking);
            }
            else if (distanceToPlayer > enemy.maxVisionDistance)
            {
                enemy.TransitionTo(
                    new PatrollingState(),
                    State.Patrolling);
            }
            else if (distanceToPlayer < enemy.positionThreshold &&
                     !enemy.canSeePlayer)
            {
                enemy.TransitionTo(
                    new PatrollingState(),
                    State.Patrolling);
            }
        }
    }

    private sealed class AttackingState : EnemyState
    {
        public override void Enter()
        {
            enemy.idleTimeCounter = enemy.idleTime;
            enemy.agent.ResetPath();
        }

        public override void Update()
        {
            enemy.Shoot();

            float distanceToPlayer = Vector3.Distance(
                enemy.transform.position,
                enemy.playerTranform.position);

            if (distanceToPlayer > enemy.attackDistance ||
                !enemy.canSeePlayer)
            {
                if (enemy.health < enemy.minChasingHealth)
                {
                    enemy.TransitionTo(
                        new PatrollingState(),
                        State.Patrolling);
                }
                else
                {
                    enemy.TransitionTo(
                        new ChasingState(),
                        State.Chasing);
                }
            }
        }
    }

    private void TransitionTo(EnemyState nextState, State stateValue)
    {
        if (currentState != null && currentState.GetType() == nextState.GetType())
        {
            return;
        }

        currentState?.Exit();

        currentState = nextState;
        currentState.SetContext(this);
        state = stateValue;

        currentState.Enter();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;

        agent = GetComponent<NavMeshAgent>();
        playerTranform = GameObject.FindWithTag("Player").GetComponent<Transform>();

        GameObject patrolPointParent = GameObject.FindWithTag("PatrolPoint");
        patrolPoints = patrolPointParent.GetComponentsInChildren<Transform>().Where(t => t != patrolPointParent.transform).ToArray();

        TransitionTo(new IdleState(), State.Idle);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Damage")
        {
            health -= 10;

            if (health <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(Blink());
            }
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    IEnumerator Blink()
    {
        rend.material = hitMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = originalMaterial;
    }

    private void Update()
    {
        LookForPlayer();

        currentState?.Update();

        rb.linearVelocity = Vector3.zero;

        LookAtPlayer();
        SetLastKnownPlayerPosition();
    }

    private void LookForPlayer()
    {
        Vector3 directionToPlayer = playerTranform.position - transform.position;

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, maxVisionDistance))
        {
            canSeePlayer = hit.transform == playerTranform;

            if (canSeePlayer && state != State.Attacking)
            {
                TransitionTo(new ChasingState(), State.Chasing);
            }
        }
    }

    private void LookAtPlayer()
    {
        if (canSeePlayer)
        {
            transform.LookAt(new Vector3(playerTranform.position.x, playerTranform.position.y, playerTranform.position.z));
        }
    }

    private void SetLastKnownPlayerPosition()
    {
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTranform.position;
        }
    }

    private void Shoot()
    {
        if (Time.time > lastShotTime + fireRate)
        {
            Vector3 directionToPlayer = playerTranform.position - transform.position;
            directionToPlayer.Normalize();

            Quaternion bulletRotation = Quaternion.LookRotation(directionToPlayer);

            float maxInaccuracy = 10f;
            float currentInaccuracy = bloom * maxInaccuracy;
            float randomJaw = Random.Range(-currentInaccuracy, currentInaccuracy);
            float randomPitch = Random.Range(-currentInaccuracy, currentInaccuracy);

            bulletRotation *= Quaternion.Euler(randomPitch, randomJaw + 90, 0f);

            AudioManager.Instance.PlaySFX(shootingSFX, 0.25f);

            Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletRotation);
            Instantiate(weaponFlash, bulletSpawnPoint.position, bulletRotation);
            lastShotTime = Time.time;
        }
    }

}
