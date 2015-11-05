using UnityEngine;
using System.Collections;

/// <summary>
/// Script for the Roadrunner enemy.
/// </summary>
public class RoadrunnerEnemy : BaseEnemy 
{
    // Array for the enemies.
    protected Transform[] enemies;

    [Space(5)]
    [Header("Particles & Sounds")]

    [Tooltip("Explosion particles.")]
    [SerializeField]
    protected GameObject explosionParticles;

    [Tooltip("Explosion sound.")]
    [SerializeField]
    protected AudioClip explosionSound;

    /// <summary>
    /// Attack method.
    /// </summary>
    public override void Attack()
    {
        Transform[] enemies = GetAllEnemiesInRange(attackRange);

        foreach (Transform enemy in enemies)
        {
            if (enemy.GetComponent<MonoBehaviour>() is BasePlayer)
            {
                BasePlayer e = (enemy.GetComponent<MonoBehaviour>() as BasePlayer);

                // Deal damage to the enemy
                e.TakeDamage(MeleeAttackDamage, this);
            }
        }

        //Explosion
        if (explosionParticles != null)
        { 
            GameObject explosion = Instantiate(explosionParticles);
            explosion.transform.position = transform.position;
        }

        // Sound
        if (explosionSound != null)
            SoundManager.SoundManagerInstance.Play(explosionSound, transform.position);

        // Camera shake
        CameraManager.CameraReference.ShakeOnce();

        OnEnemyDeath();
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Returns all enemy transforms in the given range.
    /// </summary>
    /// <param name="range">Range</param>
    /// <returns></returns>
    protected Transform[] GetAllEnemiesInRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        Transform[] enemies = new Transform[hits.Length];

        for (int i = 0; i < hits.Length; i++)
            enemies[i] = hits[i].transform;

        //Debug.Log("ChickenBehaviour: Enemies in ragne - " + enemies.Length);

        return enemies;
    }
}