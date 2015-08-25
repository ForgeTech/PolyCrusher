using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all projectiles.
/// </summary>
public abstract class Projectile : MonoBehaviour
{
    #region Class members
    //The direction of the projectile.
    protected Vector3 direction;

    //The damage of the projectile
    [SerializeField]
    protected int damage;

    [Space(5)]
    [Header("Death particle prefab")]
    // Prefab for the death particles.
    [SerializeField]
    protected GameObject deathParticlePrefab;

    // The owner of the projectile.
    protected MonoBehaviour ownerScript;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the direction of the projectile.
    /// </summary>
    public Vector3 Direction
    {
        get { return this.direction; }
        set { this.direction = value; }
    }

    /// <summary>
    /// Gets or sets the damage of the projectile.
    /// </summary>
    public int Damage
    {
        get { return this.damage; }
        set { this.damage = value; }
    }

    /// <summary>
    /// Gets or sets the owner script.
    /// </summary>
    public MonoBehaviour OwnerScript
    {
        get { return this.ownerScript; }
        set { this.ownerScript = value; }
    }
    #endregion

    /// <summary>
    /// Update method of the projectile.
    /// </summary>
    protected virtual void Update()
    {
        // Update the shoot mechanic.
        Shoot();
    }

    /// <summary>
    /// Destroys the projectile when it off the camera screen.
    /// </summary>
    protected virtual void OnBecameInvisible()
    {
        // Destroy projectile.
        if(gameObject.activeSelf)
            Destroy(this.gameObject);
    }

    /// <summary>
    /// The actual shoot implementation of the projectile.
    /// </summary>
    protected abstract void Shoot();

    /// <summary>
    /// Spawns the death particles.
    /// </summary>
    protected abstract void SpawnDeathParticle(Vector3 position);
}
