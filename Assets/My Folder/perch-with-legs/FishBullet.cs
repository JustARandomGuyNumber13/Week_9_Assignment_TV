using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class FishBullet : MonoBehaviour
{
    [SerializeField] private GameObject explo1, explo2;
    [SerializeField] private GameObject[] bodyParts;
    [SerializeField] private Animator anim;
    [SerializeField] LayerMask target;

    private NavMeshAgent ai;
    private Rigidbody rb;
    private GameObject chasingTarget;
    bool isChasing;

    float damageAmount;
    float speed;
    float knockback;
    float lifetime;

    private void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (isChasing) SpiderModeChasing();
        else // If collide with Jeff, the target
        {
            var target = DetectTarget();    // Knock back
            if (target != null)
            {
                var direction = GetComponent<Rigidbody>().velocity;
                direction.Normalize();
                target.Hit(direction * knockback, damageAmount);
                ActivateSpiderMode();
            }
        }
    }

    /* Alternative for detecting collision since I'm already using layer to keep the bullet on the surface */
    private Damageable DetectTarget()   
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.5f, transform.forward, 0f);
        foreach(RaycastHit hit in hits) 
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Damageable other = hit.collider.GetComponent<Damageable>();
                if (other != null)
                    return other;
            }
        }
        return null;
    }
    public void Initialize(float damage, float velocity, float life, float force)
    {
        damageAmount = damage;
        speed = velocity;
        lifetime = life;
        knockback = force;
        rb.velocity = transform.forward * speed;
        Invoke("ActivateSpiderMode", lifetime);                                         
    }
    private void ActivateSpiderMode()   // EXPLOSION 1 = > ACTIVATE SPIDER MODE
    {
        explo1.SetActive(true);
        anim.SetBool("isActivate", true);
        transform.eulerAngles = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        chasingTarget = GameObject.Find("Jeff_Target");
        isChasing = true;
        ai.enabled = true;
        Invoke("Demolition", 5);
    }
    private void SpiderModeChasing()    // Chase player while in Spider mode
    {
        transform.LookAt(chasingTarget.transform);
        if (ai.isOnNavMesh)
            ai.SetDestination(chasingTarget.transform.position);
        else
            Demolition();
    }
    private void Demolition()   // EXPLOSION 2 = > DESTROY
    {
        explo2.SetActive(true);
        anim.SetBool("isDemo", true);
        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        var target = DetectTarget();    // Knock back
        if (target != null)
        {
            var direction = GetComponent<Rigidbody>().velocity;
            direction.Normalize();
            target.Hit(direction * knockback, 999);
            ActivateSpiderMode();
        }

        foreach (GameObject part in bodyParts)  // Deactivate Mesh after exploded (The fish, not the explosion)
            part.GetComponent<SkinnedMeshRenderer>().enabled = false;
        Invoke("DestroySelf", 2);
    }
    private void DestroySelf()
    { 
        Destroy(this.gameObject);
    }
}
