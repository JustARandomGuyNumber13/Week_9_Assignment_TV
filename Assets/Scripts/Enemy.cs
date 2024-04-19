using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    Vector3 origin;

    public enum AnimationState
    {
        idle, walk, run, attack
    }
    private AnimationState animState;
    private NavMeshAgent agent;
    private Vector3 orgPos, curPatrolPos;
    [SerializeField]private Transform player;
    [SerializeField]private float patrolRange, sightRange, chaseRange, attackRange;
    [SerializeField]private float patrolDelay, chaseDelay, attackDelay; // ChaseDelay: Ammount of time stop after player exited "chaseRange" then return to patrolRange, ignore sightRange
    private float patrolDelayCount, chaseDelayCount, attackDelayCount;
    private bool isInSightRange, isInChaseRange, isInPatrolRange, isInAttackRange, isChangeState;
    
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        origin = transform.position;
        agent = GetComponent<NavMeshAgent>();
        animState = AnimationState.idle;
        ToNewPatrolLocation();
    }
    void Update()
    {
        isInAttackRange = Vector3.Distance(player.position, transform.position) <= attackRange ? true : false;
        isInSightRange = Vector3.Distance(player.position, transform.position) <= sightRange ? true : false;
        isInChaseRange = Vector3.Distance(transform.position, orgPos) <= chaseRange ? true : false;
        isInPatrolRange = Vector3.Distance(transform.position, orgPos) <= patrolRange ? true : false;
        attackDelayCount += Time.deltaTime;
        ChasePlayer();
        AttackPlayer();
        agent.SetDestination(curPatrolPos);
    }
    private void AttackPlayer()
    {
        if (attackDelayCount >= attackDelay && isInAttackRange)
        {
            print("Attack Player");
            attackDelayCount = 0;
        }
    }
    private void ChasePlayer()
    {
        if (!isChangeState)
        {
            if (isInSightRange && isInChaseRange)   // Start to chase player
            {
                animState = AnimationState.run;
                //agent.SetDestination(player.position);
                curPatrolPos = player.position;
            }
            else
            {
                /* Need another condition when player exit SightRange while in PatrolRAnge*/
                if (animState == AnimationState.run)  
                {
                    animState = AnimationState.idle;
                    //agent.SetDestination(transform.position);
                    curPatrolPos = transform.position;
                    isChangeState = true;
                }
            }
        }
        else
        {
            if (chaseDelayCount < chaseDelay)   // Stop "delay" amount then return to patrolRange
            {
                chaseDelayCount += Time.deltaTime;
            }
            else // Return to patrolRange
            {
                animState = AnimationState.run;
                //agent.SetDestination(orgPos);
                curPatrolPos = orgPos;
            }

            if (isInPatrolRange)    // Be able to chase again && Start Patrol again
            {
                chaseDelayCount = 0;
                isChangeState = false;
                Invoke("ToNewPatrolLocation", patrolDelay);
            }
        }
    }
    private void ToNewPatrolLocation()
    {
        if (!isInSightRange)
        {
            float randX = Random.Range(orgPos.x - patrolRange + 0.1f, orgPos.x + patrolRange - 0.1f);   // 0.1f is the off set in case patrol cordination is out of range
            float randz = Random.Range(orgPos.z - patrolRange + 0.1f, orgPos.z + patrolRange - 0.1f);
            curPatrolPos = new Vector3(randX, 0, randz);
            animState = AnimationState.walk;
            Invoke("ToNewPatrolLocation", patrolDelay);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(orgPos, chaseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(orgPos, patrolRange);

    }

    public void ApplyKnockback(Vector3 knockback)
    {
        GetComponent<Rigidbody>().AddForce(knockback, ForceMode.Impulse);
    }
    public void Respawn()
    {
        transform.position = origin;
    }
}
