    using UnityEngine;
using System.Collections;

/// <summary>
/// Implements a machine gun based on the weapon class.
/// </summary>
public class WeaponMachinegun : Weapon 
{
    //Prefab name of the bullet of the weapon.
    [SerializeField]
    protected string bulletPrefabName = "Bullet/BulletTest";

    /// <summary>
    /// The Shoot mechanic of the machine gun.
    /// </summary>
    public override void Use()
    {
        if (shootIsAllowed)
        {
            base.Use();

            GameObject g = Instantiate(Resources.Load(bulletPrefabName)) as GameObject;
            Bullet bullet;

            if (g != null && g.GetComponent<MonoBehaviour>() is Bullet)
            {
                bullet = g.GetComponent<MonoBehaviour>() as Bullet;
                bullet.OwnerScript = this.OwnerScript;
                bullet.name = "MachineGunBullet";
                bullet.Damage = this.WeaponDamage;
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.LookRotation(transform.forward);
                float speed = bullet.BulletSpeed;

                //Debug.DrawLine(transform.position, transform.forward * 5f);

                // Random rotation
                Quaternion rotation = Quaternion.Euler(0, Random.Range(-weaponAccuracy, weaponAccuracy), 0);
                Vector3 v = transform.forward;
                Vector3 rotationVector = rotation * v;
                
                bullet.Shoot(rotationVector, speed);
            }

            shootIsAllowed = false;
            StartCoroutine(WaitForNextShot());
        }
    }
}
