using UnityEngine;
using System.Collections;

/// <summary>
/// Boss shield behaviour.
/// Detects bullet impact and spawns a new enemy bullet.
/// </summary>
public class BossShield : MonoBehaviour
{
    [SerializeField]
    protected GameObject enemyBullet;

    /// <summary>
    /// Detects if a player bullet entered.
    /// </summary>
    /// <param name="other"></param>
    protected void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            if (other != null)
            {
                Vector3 forward = other.transform.forward;
                Vector3 position = other.transform.position;
                
                Vector3 reflectedVector = Vector3.Reflect(forward, transform.parent.transform.forward);

                // Create bullet
                GameObject g = ObjectsPool.Spawn(enemyBullet, Vector3.zero, enemyBullet.transform.rotation);
                Bullet b = g.GetComponent<MonoBehaviour>() as Bullet;

                b.OwnerScript = this;
                g.name = "RangedBullet";
                g.transform.position = new Vector3(position.x, 0.6f, position.z);
                g.transform.rotation = Quaternion.LookRotation(reflectedVector);

                b.Damage = b.Damage;
                b.Shoot(reflectedVector, b.BulletSpeed);
            }
        }
    }
}
