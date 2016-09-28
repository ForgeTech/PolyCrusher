using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a bullet with a specific flying time.
/// </summary>
public class Bullet : Projectile
{
    #region Members and Properties
    [Space(5)]
    [Header("Bullet parameters")]
    // The life time of the bullet in seconds.
    [SerializeField]
    protected float bulletLifeTime;

    // The speed of the bullet.
    [SerializeField]
    protected float bulletSpeed;

    // The original bullet speed.
    protected float originalBulletSpeed;

    // The variation of the bullet speed.
    [SerializeField]
    protected float bulletSpeedVariation = 0;

    // The force to push away the target.
    [SerializeField]
    protected float pushAwayForce = 2.5f;

    [Space(5)]
    [Header("Targeting values")]
    // The tag of the hit target.
    [SerializeField]
    protected string targetTag;

    /// <summary>
    /// Gets or sets the bullet life time.
    /// </summary>
    public float BulletLifeTime
    {
        get { return this.bulletLifeTime; }
        set { this.bulletLifeTime = value; }
    }

    /// <summary>
    /// Gets or sets the target hit tag.
    /// </summary>
    public string TargetTag
    {
        get { return this.targetTag; }
        set { this.targetTag = value; }
    }

    /// <summary>
    /// Gets or sets the speed of the bullet.
    /// </summary>
    public float BulletSpeed
    {
        get { return this.bulletSpeed; }
        set { this.bulletSpeed = value; }
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        originalBulletSpeed = bulletSpeed;
    }

    /// <summary>
    /// Start method of Mono Behaviour.
    /// </summary>
    void Start()
    {
        
    }

    /// <summary>
    /// Enabling routines
    /// </summary>
    protected virtual void OnEnable()
    {
        //If there is no circle collider, generate one.
        if (GetComponent<SphereCollider>() == null)
        {
            SphereCollider s = gameObject.AddComponent<SphereCollider>();
            s.isTrigger = true;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        LeanTween.scale(this.gameObject, originalScale, 0.1f).setEase(AnimCurveContainer.AnimCurve.upscale);

        StartCoroutine(DestroyProjectileAfterTime(BulletLifeTime));
    }

    /// <summary>
    /// The shoot mechanic of the bullet.
    /// Call this to start a shot.
    /// </summary>
    protected override void Shoot()
    {
        // Translate bullet.
        transform.Translate(Direction * bulletSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// The actual shoot implementation with a direction vector.
    /// </summary>
    /// <param name="direction"></param>
    public virtual void Shoot(Vector3 direction, float bulletSpeed)
    {
        // Set parameters.
        this.Direction = direction;
        this.BulletSpeed = bulletSpeed - Random.Range(0f, bulletSpeedVariation);

        // Launch bullet.
        Shoot();
    }

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
                ((IDamageable)m).TakeDamage(Damage, this.OwnerScript,  transform.transform.forward, false);
                //Debug.Log("Bullet: " + other.name + " was hit!");

                SpawnDeathParticle(transform.position);
                ApplyExplosionForce(other.gameObject, transform.position);

                DestroyProjectile();
            }
        }

        if (other.tag == "Terrain" || other.gameObject.layer == LayerMask.NameToLayer("Props"))
        {
            SpawnDeathParticle(transform.position);
            DestroyProjectile();
        }

        if (TargetTag == "Enemy")
        {
            if (other.tag == "BossShield")
            {
                SpawnDeathParticle(transform.position);
                DestroyProjectile();
            }
        }
    }

    /// <summary>
    /// Spawns the death particles.
    /// </summary>
    protected override void SpawnDeathParticle(Vector3 position)
    {
        if (deathParticlePrefab != null)
        {
            GameObject particle = Instantiate(deathParticlePrefab) as GameObject;
            particle.transform.position = position;

            if (particle.GetComponent<ParticleSystem>() != null)
                particle.GetComponent<ParticleSystem>().Play();
        }
    }

    /// <summary>
    /// Applies force to the hit object.
    /// </summary>
    /// <param name="obj">Object to apply force</param>
    /// <param name="hitPosition">Hit position (Source of the force)</param>
    protected void ApplyExplosionForce(GameObject obj, Vector3 hitPosition)
    {
        obj.GetComponent<Rigidbody>().AddExplosionForce(pushAwayForce, hitPosition, 5f, 0f, ForceMode.Impulse);
    }

    /// <summary>
    /// Destroys the bullet immediatly.
    /// </summary>
    protected override void DestroyProjectile()
    {
        bulletSpeed = originalBulletSpeed;
        base.DestroyProjectile();
    }
}
