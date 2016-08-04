using UnityEngine;
using System.Collections;
using System;

public class PerlinRayProjectile : Projectile
{
    [Header("Ray settings")]
    [SerializeField]
    protected float maxLength = 5f;

    // The random offset of the ray.
    [SerializeField]
    protected float lengthOffset = 10f;

    // The life time of the ray.
    [SerializeField]
    protected float rayLifeTime = 0.3f;

    [SerializeField]
    protected float rayAnimationSpeed = 2f;

    [Header("Particle settings")]
    [SerializeField]
    protected float particleSize = 0.6f;

    [SerializeField]
    [Tooltip("Particle quantity")]
    protected int particleQuantity = 100;

    [Header("Perlin noise settings")]
    [SerializeField]
    protected float speed = 1f;

    [SerializeField]
    protected float scale = 1f;

    [Header("General settings")]
    [SerializeField]
    protected int targetLayer;

    [SerializeField]
    protected AnimationCurve sizeCurve;

    private Perlin noise = new Perlin();
    private float oneOverQuantity;      // 1 / particleQuantity
    private Particle[] particles;

    private ParticleEmitter pEmitter;

    // Noise variables
    private float timeX;
    private float timeY;
    private float timeZ;

    private Vector3 position;
    private Vector3 offset = Vector3.zero;

    private Vector3 targetPosition = Vector3.zero;

    private float currentLifeTime = 0f;

    // Determines if the ray was already shot or not.
    private bool rayShot = false;

    /// <summary>
    /// Gets or sets the max length.
    /// </summary>
    public float MaxLength
    {
        get { return this.maxLength; }
        set { this.maxLength = value; }
    }

    /// <summary>
    /// Gets or sets the target layer of the ray.
    /// </summary>
    public int TargetLayer
    {
        get { return this.targetLayer; }
        set { this.targetLayer = value; }
    }

    protected override void Awake ()
    {
        base.Awake();

        this.oneOverQuantity = 1f / particleQuantity;
        InitializeEmitter();
	}

    private void CalculateParticlePosition()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            position = Vector3.Lerp(transform.position, this.targetPosition, oneOverQuantity * (float)i);

            // Use Set() to avoid 'new Vector()'
            offset.Set(noise.Noise(timeX + position.x, timeX + position.y, timeX + position.z),
                                        noise.Noise(timeY + position.x, timeY + position.y, timeY + position.z),
                                        noise.Noise(timeZ + position.x, timeZ + position.y, timeZ + position.z));
            
            position += (offset * scale * ((float)i * oneOverQuantity));

            particles[i].position = position;
            particles[i].color = Color.white;

            // oneOverQuantity * i: The lightning ray matches at each position the size of the graph
            // currentLifeTime * rayAnimationSpeed - 0.5f: Starts with -0.5 -> So it starts in the middle of the graph and then the whole graph is shifted
            //                                                                 This causes the pulse effect.
            particles[i].size = particleSize * sizeCurve.Evaluate(oneOverQuantity * (float)i - (currentLifeTime * rayAnimationSpeed - 0.5f));
        }
    }

    private void UpdateTime()
    {
        timeX = Time.time * speed * 0.1365143f;
        timeY = Time.time * speed * 1.21688f;
        timeZ = Time.time * speed * 2.5564f;
    }

    private void InitializeEmitter()
    {
        pEmitter = GetComponent<ParticleEmitter>();
        pEmitter.emit = false;
        pEmitter.Emit(particleQuantity);

        particles = pEmitter.particles;
    }

    /// <summary>
    /// Shoots the ray with the specific params.
    /// </summary>
    /// <param name="direction">Direction of the ray.</param>
    /// <param name="maxLength">Max. length of the ray.</param>
    public void Shoot(Vector3 direction)
    {
        // Set params
        this.Direction = direction;

        // Start the shoot mechanism
        Shoot();
    }

    protected override void Shoot()
    {
        if (!rayShot)
        {
            CheckTarget();
            DestroyProjectile();
            rayShot = true;
        }
        else
        {
            // Update ray
            GenerateRay(this.targetPosition);
        }

        currentLifeTime += Time.deltaTime;
    }

    private void CheckTarget()
    {
        RaycastHit hitInfo;
        // Boss shield layer
        if (targetLayer == 9 && Physics.Raycast(transform.position, Direction, out hitInfo, maxLength, 1 << 16, QueryTriggerInteraction.Collide))
        {
            GenerateRay(hitInfo.point);

            BossShield shield = hitInfo.transform.GetComponent<BossShield>();
            if (shield != null)
            {
                shield.CreateEnemyRay(hitInfo.point, Direction);
            }
        }
        else if (Physics.Raycast(transform.position, Direction, out hitInfo, maxLength, 1 << targetLayer))
        {
            GenerateRay(hitInfo.point);
        }
        else if (Physics.Raycast(transform.position, Direction, out hitInfo, maxLength, 1 << 10))
        {
            GenerateRay(hitInfo.point);

            // Check for destructible item
            PolyExplosionThreeDimensional destructible = hitInfo.transform.gameObject.GetComponent<PolyExplosionThreeDimensional>();
            if (destructible != null)
            {
                destructible.DecrementHealth();
                SpawnDeathParticle(hitInfo.transform.position);
            }
        }
        else
        {
            float distanceOffset = UnityEngine.Random.Range(0f, lengthOffset);

            //Define first an last point.
            GenerateRay(transform.position + Direction * (maxLength + distanceOffset));
        }

        // Check if the hit-object can take damage
        if (hitInfo.transform != null && hitInfo.transform.gameObject.GetComponent<MonoBehaviour>() is IDamageable)
        {
            (hitInfo.transform.gameObject.GetComponent<MonoBehaviour>() as IDamageable).TakeDamage(Damage, this.OwnerScript);
            SpawnDeathParticle(hitInfo.transform.position);
        }
    }

    private void GenerateRay(Vector3 targetPosition)
    {
        UpdateTime();

        if (particles != null)
        {
            this.targetPosition = targetPosition;
            CalculateParticlePosition();

            // Set calculated particle positions
            pEmitter.particles = particles;
        }
    }

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

    protected override void DestroyProjectile()
    {
        Destroy(this.gameObject, rayLifeTime);
    }
}