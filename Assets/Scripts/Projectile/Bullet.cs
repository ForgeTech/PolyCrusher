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

    /// <summary>
    /// Start method of Mono Behaviour.
    /// </summary>
    void Start()
    {
        //If there is no circle collider, generate one.
        if (GetComponent<SphereCollider>() == null)
        {
            SphereCollider s = gameObject.AddComponent<SphereCollider>();
            s.isTrigger = true;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        StartCoroutine(transform.ScaleTo(originalScale, 0.1f, AnimCurveContainer.AnimCurve.upscale.Evaluate));

        Destroy(this.gameObject, BulletLifeTime);
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
                ((IDamageable)m).TakeDamage(Damage, this.OwnerScript);
                //Debug.Log("Bullet: " + other.name + " was hit!");

                SpawnDeathParticle(transform.position);
                ApplyExplosionForce(other.gameObject, transform.position);                

                Destroy(this.gameObject);
            }
        }

        if (other.tag == "Terrain" || other.gameObject.layer == LayerMask.NameToLayer("Props"))
        {
            SpawnDeathParticle(transform.position);
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Spawns the death particles.
    /// </summary>
    protected override void SpawnDeathParticle(Vector3 position)
    {
        GameObject particle = Instantiate(deathParticlePrefab) as GameObject;
        particle.transform.position = position;

        if(particle.GetComponent<ParticleSystem>() != null)
            particle.GetComponent<ParticleSystem>().Play();
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
}
