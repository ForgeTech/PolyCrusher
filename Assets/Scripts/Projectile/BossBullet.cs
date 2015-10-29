using UnityEngine;
using System.Collections;

public class BossBullet : Bullet
{
    // Death area prefab.
    [Space(4)]
    [Header("Boss bullet settings")]
    [Tooltip("Prefab of the death area.")]
    [SerializeField]
    protected GameObject deathArea;

    /// <summary>
    /// On trigger enter bullet behaviour.
    /// </summary>
    /// <param name="other">Collider</param>
    void OnTriggerEnter(Collider other)
    {
        // Check if the hit target was hit.
        if (other.tag == targetTag)
        {
            MonoBehaviour m = other.gameObject.GetComponent<MonoBehaviour>();

            if (m != null && m is IDamageable)
            {
                ((IDamageable)m).TakeDamage(Damage, this.OwnerScript);
                //Debug.Log("Bullet: " + other.name + " was hit!");

                // Area of damage
                CreateAreaOfDamage();

                SpawnDeathParticle(transform.position);
                ApplyExplosionForce(other.gameObject, transform.position);

                Destroy(this.gameObject);
            }
        }

        if (other.tag == "Terrain" || other.gameObject.layer == LayerMask.NameToLayer("Props"))
        {
            // Area of damage
            CreateAreaOfDamage();

            Destroy(this.gameObject);
            SpawnDeathParticle(transform.position);
        }
    }

    /// <summary>
    /// Instantiates the area of damage.
    /// </summary>
    private void CreateAreaOfDamage()
    {
        //Spawn directly on the NavMesh
        NavMeshHit hit;

        // Sample bullet position on NavMesh.
        bool posFound = NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas);

        // Only instantiate if position was found
        if (posFound)
        {
            GameObject g = Instantiate(deathArea) as GameObject;

            g.transform.position = hit.position;
            g.GetComponent<BossMeleeScript>().InitMeleeScript(this.OwnerScript.GetComponent<BossEnemy>());
        }
    }
}
