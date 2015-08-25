using UnityEngine;
using System.Collections;

/// <summary>
/// Implents the pistol shoot behaviour. 
/// This scripts need to child gameobjects for the alternate bullet spawning.
/// </summary>
public class WeaponPistol : Weapon 
{
    //Prefab name of the bullet of the weapon.
    [SerializeField]
    protected string bulletPrefabName = "Bullet/BulletTest";

    // Spawn points of the bullets.
    protected Transform[] bulletSpawns;

    // Boolean for the spawn point alteration.
    private bool alternateSpawn = true;

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

            GameObject g = Instantiate(Resources.Load(bulletPrefabName)) as GameObject;
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
                    spawn = bulletSpawns[1];
                else
                    spawn = bulletSpawns[2];

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
