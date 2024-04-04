using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override bool AttemptFire()
    {
        if (!base.AttemptFire())
            return false;
        ShotGunEffect();
        anim.SetTrigger("shoot");
        elapsed = 0;
        ammo -= 1;

        return true;
    }
    private void ShotGunEffect()
    {
        int randomBulletAmount = (int) Random.Range(4, 8);
        for (int i = 0; i < randomBulletAmount; i++)
        {
            float dirX = Random.Range(-5, 5);
            float dirY = Random.Range(-5, 5);
            Quaternion randomDirection = Quaternion.Euler(new Vector3(dirX, dirY, 0) + gunBarrelEnd.transform.eulerAngles);
            var b = Instantiate(bulletPrefab, gunBarrelEnd.transform.position, randomDirection);
            b.GetComponent<Projectile>().Initialize(1, 100, 1, 3, DoThing); // version with special effect
        }
    }
    void DoThing(HitData data)
    {
        Vector3 impactLocation = data.location;

        var colliders = Physics.OverlapSphere(impactLocation, 1);
        foreach (var c in colliders)
        {
            if (c.GetComponent<Rigidbody>())
            {
                c.GetComponent<Rigidbody>().AddForce(Vector3.up * 3, ForceMode.Impulse);
            }
        }
    }
}
