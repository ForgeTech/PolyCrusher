using UnityEngine;

public class Rocket : Projectile
{
	// Area of effect
	[SerializeField]
	private float damageRadius = 1f;

    [SerializeField]
    private AnimationCurve animCurve;

    [SerializeField]
    private float maxTime = 1.2f;

    [SerializeField]
    private float curveHeightMultiplier = 2f;

    [SerializeField]
    private float linearHeight = 0.5f;
    private float currentHeight = 0f;

	// Vector of the target which the missle must approach.
	private Vector3 target;
	
	// Compute after the missle is launched (if it is true then start the method in fixedupdate().
	private bool launched = false;

	// If true then draw the damageradius gizmo.
	[SerializeField]
	private bool drawDamageRadiusGizmo = true;

	[SerializeField]
	protected float timeScale = 1f;

	// Height which the projectile shoud achieve.
	[SerializeField]
	private float lifeTime = 5.0f; 

	// Audiofile of the explosion
	[SerializeField]
	private AudioClip explosionSound;
	
	// only play once
	private bool playExplode = true;

    [SerializeField]
    protected GameObject attackVisualization;

    [SerializeField]
    protected float terrainRaycastLength = 0.3f;

    private bool alreadyTriggered = false;

    LTDescr mainTween = null;
    LTDescr heightTween = null;

    /// <summary>
    /// Gets or sets the speed of the projectile.
    /// </summary>
    public float TimeScale
	{
		get { return timeScale; }
		set
        {
            timeScale = value;
            if (mainTween != null)
                mainTween.setTime(maxTime * value);
            if(heightTween != null)
                heightTween.setTime(maxTime * value);
        }
	}

    /// <summary>
    /// Enabling routines
    /// </summary>
    protected virtual void OnEnable()
    {
        launched = false;
        playExplode = true;
        currentHeight = 0f;

        MeshRenderer meshRenderer = transform.GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = true;
    }

    public void Shoot(Vector3 target) {
        CreateAttackVisualization(target);

		this.target = new Vector3(target.x, target.y - 0.1f, target.z);
        transform.localScale = Vector3.one * 2f;
        launched = true;

        Vector3 modifiedOriginalPosition = new Vector3(transform.position.x, linearHeight, transform.position.z);
        Vector3 modifiedTargetPosition = new Vector3(target.x, linearHeight, target.z);

        mainTween = LeanTween.value(gameObject, modifiedOriginalPosition, modifiedTargetPosition, maxTime)
            .setOnStart(() => {
                launched = true;
                
            })
            .setOnUpdate((Vector3 val) => {
                Vector3 newPosition = new Vector3(val.x, modifiedOriginalPosition.y + currentHeight, val.z);
                transform.position = newPosition;
            }).setEase(LeanTweenType.easeOutSine);

        heightTween = LeanTween.value(gameObject, 0f, 1f, maxTime).setOnUpdate((float val) => {
            currentHeight = animCurve.Evaluate(val) * curveHeightMultiplier;
        }).setEase(LeanTweenType.easeOutSine);

        StartCoroutine(DestroyProjectileAfterTime(lifeTime));
	}

    private void CreateAttackVisualization(Vector3 target)
    {
        NavMeshHit hit;
        if(NavMesh.SamplePosition(target, out hit, 5f, NavMesh.AllAreas))
            Instantiate(attackVisualization, hit.position, attackVisualization.transform.rotation);
        else
            Instantiate(attackVisualization, target, attackVisualization.transform.rotation);
    }

    protected override void Shoot()
    {
        if (launched && ReachedTerrain())
            HandleCollisionRoutine();
    }

    private bool ReachedTerrain()
    {
        if (transform.position.y <= target.y)
            return true;

        return false;
    }

    private void HandleCollisionRoutine()
    {
        // Deactivate mesh renderer.
        MeshRenderer meshRenderer = transform.GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = false;

        // only calling the explosionsound method once
        if (playExplode)
        {
            if (explosionSound != null)
            {
                SoundManager.SoundManagerInstance.Play(explosionSound, transform.position, AudioGroup.Effects);
                playExplode = false;
            }

            Collider[] collidingObjects = Physics.OverlapSphere(transform.position, damageRadius, 1 << 8);

            // Handles the damage taking of the players.
            foreach (Collider objects in collidingObjects)
            {
                if (objects.tag == "Player")
                {
                    MonoBehaviour m = objects.gameObject.GetComponent<MonoBehaviour>();
                    if (m != null && m is IDamageable)
                        ((IDamageable)m).TakeDamage(damage, this);
                }
            }

            SpawnDeathParticle(new Vector3(transform.position.x, 0.05f, transform.position.z));

            // Camera Shake
            CameraManager.CameraReference.ShakeOnce();
        }
        //Destroy(this.gameObject, 0.1f);
        StartCoroutine(DestroyProjectileAfterTime(0.1f));
    }

    /// <summary>
    /// Handles the spawn of the particlesystem.
    /// </summary>
    /// <param name="position">Position of the Rocket in a 3D vector.</param>
	protected override void SpawnDeathParticle(Vector3 position)
	{
		GameObject particle = Instantiate(deathParticlePrefab) as GameObject;
		particle.transform.position = position;
	}

	/// <summary>
	/// Draws a wiresphere for debug reasons.
	/// </summary>
	private void OnDrawGizmos()
    {
		if (drawDamageRadiusGizmo)
        {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, damageRadius);
		}
	}
}
