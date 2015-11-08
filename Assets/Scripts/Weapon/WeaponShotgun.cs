using UnityEngine;
using System.Collections;

/// <summary>
/// Fires more bullets at once in a given angle.
/// </summary>
public class WeaponShotgun : Weapon 
{
    // The number of bullets which should be shot by the shotgun.
    [SerializeField]
    protected int numberOfBullets = 5;

    // Angle from one left outer bullet to the right outer bullet.
    [SerializeField]
    protected float spreadAngle = 20f;

    /// <summary>
    /// The Shoot mechanic of the shotgun.
    /// </summary>
    public override void Use()
    {
        if (shootIsAllowed)
        {
            base.Use();    

            // The angle between 2 bullets.
            float angleBetween = spreadAngle / numberOfBullets;
            //The start angle of the calculation.
            float currentAngle = -(spreadAngle / 2f);

            // Instantiate the bullets
            for (int i = 0; i < numberOfBullets; i++)
            {
                GameObject g = ObjectsPool.Spawn(bulletPrefab, Vector3.zero, bulletPrefab.transform.rotation);
                //GameObject g = Instantiate(bulletPrefab) as GameObject;
                Bullet bullet;

                if (g != null && g.GetComponent<MonoBehaviour>() is Bullet)
                {
                    bullet = g.GetComponent<MonoBehaviour>() as Bullet;
                    bullet.OwnerScript = this.OwnerScript;
                    bullet.name = "ShotgunBullet";
                    bullet.Damage = this.WeaponDamage;
                    bullet.transform.position = transform.position;
                    bullet.transform.rotation = Quaternion.LookRotation(transform.forward);
                    
                    float speed = bullet.BulletSpeed;

                    // Calculate the bullet direction
                    Quaternion rotation = Quaternion.Euler(0, Random.Range(currentAngle - weaponAccuracy, currentAngle + weaponAccuracy), 0);
                    Vector3 v = transform.forward;
                    Vector3 rotationVector = rotation * v;

                    // Shoot bullet
                    bullet.Shoot(rotationVector, speed);

                    currentAngle += angleBetween;
                }
            }

            shootIsAllowed = false;
            StartCoroutine(WaitForNextShot());
        }
    }
}
