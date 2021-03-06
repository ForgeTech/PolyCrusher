﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Implents the pistol shoot behaviour. 
/// This scripts need to child gameobjects for the alternate bullet spawning.
/// </summary>
public class WeaponPistol : Weapon 
{
    // Spawn points of the bullets.
    protected Transform[] bulletSpawns;

    // Boolean for the spawn point alteration.
    private bool alternateSpawn = true;

    [SerializeField]
    private ParticleSystem rightBulletRounds;

	protected override void Start()
    {
 	    base.Start();

        // Get bullet spawns.
        bulletSpawns = transform.GetComponentsInChildren<Transform>();
        //Debug.Log("WeaponPistol: [0]:" + bulletSpawns[0].name + ", [1]: " + bulletSpawns[1].name + ", Length: " + bulletSpawns.Length);
    }


    public override void Use()
    {
        if (shootIsAllowed)
        {
            base.Use();

            //GameObject g = Instantiate(bulletPrefab) as GameObject;
            GameObject g = ObjectsPool.Spawn(bulletPrefab, Vector3.zero, bulletPrefab.transform.rotation);
            Bullet bullet;
            Transform spawn;

            if (g != null && g.GetComponent<MonoBehaviour>() is Bullet)
            {
                bullet = g.GetComponent<MonoBehaviour>() as Bullet;
                bullet.OwnerScript = this.OwnerScript;
                bullet.name = "PistolBullet";
                bullet.Damage = this.WeaponDamage;

                // Alternate the spawnpoint of the dual pistol
                if (alternateSpawn)
                {
                    spawn = bulletSpawns[1];
                    bulletRounds.Emit(1);
                }
                else
                {
                    spawn = bulletSpawns[2];
                    rightBulletRounds.Emit(1);
                }

                alternateSpawn = !alternateSpawn;

                bullet.transform.position = spawn.position;
                bullet.transform.rotation = Quaternion.LookRotation(spawn.forward);
                float speed = bullet.BulletSpeed;

                // Random rotation
                Quaternion rotation = Quaternion.Euler(0, Random.Range(-weaponAccuracy, weaponAccuracy), 0);
                Vector3 v = spawn.forward;
                Vector3 rotationVector = rotation * v;

                bullet.Shoot(rotationVector, speed);
            }

            shootIsAllowed = false;
            StartCoroutine(WaitForNextShot());
        }
    }
}
