using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class FishBullet : MonoBehaviour
{
    [SerializeField] private GameObject explo1, explo2;
    [SerializeField] private GameObject[] bodyParts;
    [SerializeField] private Animator anim;
    private NavMeshAgent ai;
    private Rigidbody rb;
    private GameObject chasingTarget;
    bool isChasing;

    float damageAmount;
    float speed;
    float knockback;
    float lifetime;

    private void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (isChasing) SpiderModeChasing();
    }
    private void OnTriggerEnter(Collider other)                             // Explosion 1
    {
        var target = other.gameObject.GetComponent<Damageable>();
        if (target != null)
        {
            var direction = GetComponent<Rigidbody>().velocity;
            direction.Normalize();
            target.Hit(direction * knockback, damageAmount);
            ActivateSpiderMode();
        }
    }
    public void Initialize(float damage, float velocity, float life, float force)
    {
        damageAmount = damage;
        speed = velocity;
        lifetime = life;
        knockback = force;
        GetComponent<CapsuleCollider>().isTrigger = false;
        rb.velocity = transform.forward * speed;
        Invoke("ActivateSpiderMode", lifetime);                                         // Explosion 1
    }
    private void SpiderModeChasing()
    {
        transform.LookAt(chasingTarget.transform);
        if (ai.isOnNavMesh)
            ai.SetDestination(chasingTarget.transform.position);
        else
            Demolition();
    }
    private void ActivateSpiderMode()                   // Explosion 1 => Activate Spider Mode => Explosion 2
    {
        chasingTarget = GameObject.Find("Jeff_Target");
        transform.eulerAngles = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        anim.SetBool("isActivate", true);
        explo1.SetActive(true);
        isChasing = true;
        ai.enabled = true;
        Invoke("Demolition", 7);
    }
    private void Demolition()                           // Explosion 2 => Destroy self
    {
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        anim.SetBool("isDemo", true);
        explo2.SetActive(true);
        foreach (GameObject part in bodyParts)
            part.GetComponent<SkinnedMeshRenderer>().enabled = false;
        Invoke("DestroySelf", 2);
    }
    private void DestroySelf()
    { 
        Destroy(this.gameObject);
    }
}
