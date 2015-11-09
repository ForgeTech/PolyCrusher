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

    [SerializeField]
    protected GameObject enemyRay;

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

    /// <summary>
    /// Creates a ray based on the input position and the forward vector of the
    /// original ray.
    /// </summary>
    /// <param name="position">Start position</param>
    /// <param name="originalForward">Original forward</param>
    public void CreateEnemyRay(Vector3 position, Vector3 originalForward)
    {
        GameObject g = Instantiate(enemyRay) as GameObject;
        RayProjectile ray;

        if (g != null && g.GetComponent<MonoBehaviour>() is RayProjectile)
        {
            Debug.Log("Instantiate enemy ray");

            ray = g.GetComponent<MonoBehaviour>() as RayProjectile;
            ray.OwnerScript = null;
            ray.Damage = ray.Damage;
            ray.name = "RayBullet";
            ray.transform.position = new Vector3(position.x, 0.6f, position.z);
            Vector3 newForward = new Vector3(originalForward.x, 0, originalForward.z);

            Vector3 reflectedVector = Vector3.Reflect(newForward, transform.parent.transform.forward);

            //Shoot
            ray.Shoot(reflectedVector.normalized);
        }
    }
}
