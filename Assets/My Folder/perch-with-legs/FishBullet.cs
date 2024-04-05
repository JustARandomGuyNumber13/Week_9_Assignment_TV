using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FishBullet : MonoBehaviour
{
    float damageAmount;
    float speed;
    float knockback;
    float lifetime;
    UnityAction<HitData> OnHit;

    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.gameObject.GetComponent<Damageable>();
        if (target != null)
        {
            var direction = GetComponent<Rigidbody>().velocity;
            direction.Normalize();

            Debug.Log("hit enemy trigger");
            target.Hit(direction * knockback, damageAmount);

            HitData hd = new HitData();
            hd.target = target;
            hd.direction = direction;
            hd.location = transform.position;

            OnHit?.Invoke(hd);
        }

        Destroy(gameObject);
    }
    public void Initialize(float damage, float velocity, float life, float force, UnityAction<HitData> onHit)
    {
        damageAmount = damage;
        speed = velocity;
        lifetime = life;
        knockback = force;
        OnHit += onHit;

        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        Destroy(gameObject, lifetime);

    }
    private void Demolition()
    {
        anim.SetBool("isDemo", true);
        Invoke("DestroySelf", 1);
    }
    private void DestroySelf()
    { 
        Destroy(this.gameObject);
    }
}
