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
    private Vector3 orgPos, curDestination;
    [SerializeField]private Transform player;
    [SerializeField]private float patrolRange, sightRange, chaseRange, attackRange;
    [SerializeField]private float patrolDelay, chaseDelay, attackDelay; // ChaseDelay: Ammount of time stop after player exited "chaseRange" then return to patrolRange, ignore sightRange
    private float chaseDelayCount, attackDelayCount, knockBackTick;
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
        knockBackTick += Time.deltaTime;
        if(!Rigidbody.isKinematic && knockBackTick > 0.2f) Rigidbody.isKinematic = true;    // Prevent AI bug from knock back effect

        ChasePlayer();
        AttackPlayer();
        agent.SetDestination(curDestination);
    }
    private void AttackPlayer()
    {
        if (attackDelayCount >= attackDelay && isInAttackRange)
        {
            //print("Attack Player");
            animState = AnimationState.attack;
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
                curDestination = player.position;
            }
            else
            {
                if (animState == AnimationState.run)  
                {
                    animState = AnimationState.idle;
                    curDestination = transform.position;
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
                curDestination = orgPos;
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
        if (!isInSightRange && !isChangeState)
        {
            float randX = Random.Range(orgPos.x - patrolRange + 0.1f, orgPos.x + patrolRange - 0.1f);   // 0.1f is the off set in case patrol cordination is out of range
            float randz = Random.Range(orgPos.z - patrolRange + 0.1f, orgPos.z + patrolRange - 0.1f);
            curDestination = new Vector3(randX, 0, randz);
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
        Rigidbody.isKinematic = false;
        GetComponent<Rigidbody>().AddForce(knockback, ForceMode.Impulse);
        knockBackTick = 0;
    }
    public void Respawn()
    {
        transform.position = origin;
    }
}
