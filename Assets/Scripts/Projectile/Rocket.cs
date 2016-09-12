using UnityEngine;
using System.Collections;

public class Rocket : Projectile {
	// Area of effect
	[SerializeField]
	private float damageRadius = 1f;

	// Vector of the target which the missle must approach.
	private Vector3 target;
	
	// Compute after the missle is launched (if it is true then start the method in fixedupdate().
	private bool launched = false;
	
	// Defines if the missle is targeting the actual target on the floor, else it is heading 
	// to the airpoint.
	private bool headingTarget = false;
	
	// Tweak to adjust top speed.
	[SerializeField]
	private float sensitivity = 0.1f;

	// If true then draw the damageradius gizmo.
	[SerializeField]
	private bool drawDamageRadiusGizmo = true;

	// If true then draw the midpoint gizmo.
	[SerializeField]
	private bool drawMidpointGizmo = true;

	// Velocity of the projectile
	[SerializeField]
	protected float bulletSpeed = 150.0f;

	// Height which the projectile shoud achieve.
	[SerializeField]
	private float height = 10.0f; 

	// Height which the projectile shoud achieve.
	[SerializeField]
	private float lifeTime = 5.0f; 

	// true if the code is executed the first time
	private bool firstTime = true;

	// Actual sensitivity for the SLerp
	private float actSensitivity;

	// Prefab for the death particles.
	[SerializeField]
	private GameObject midAirExplosionParticlePrefab;

	// Audiofile of the explosion
	[SerializeField]
	private AudioClip explosionSound;
	
	// only play once
	private bool playExplode = true;

	// Delta length of the transform to the target
	private float deltaX;
	private float deltaZ;

	// The actual target
	private Vector3 targetPos;

    [SerializeField]
    protected GameObject attackVisualization;

    private bool alreadyTriggered = false;

	/// <summary>
	/// Gets or sets the speed of the projectile.
	/// </summary>
	public float BulletSpeed
	{
		get { return this.bulletSpeed; }
		set { this.bulletSpeed = value; }
	}

	/// <summary>
	/// Gets or sets the sensetivity of the projectile.
	/// </summary>
	public float Sensitivity
	{
		get { return this.sensitivity; }
		set { this.sensitivity = value; }
	}

    /// <summary>
    /// Enabling routines
    /// </summary>
    protected virtual void OnEnable()
    {
        launched = false;
        headingTarget = false;
        firstTime = true;
        playExplode = true;

        MeshRenderer meshRenderer = transform.GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = true;
    }

    public void Shoot(Vector3 target) {
        CreateAttackVisualization(target);
		this.target = new Vector3(target.x, target.y - 0.1f, target.z);
		launched = true;
        StartCoroutine(DestroyProjectileAfterTime(lifeTime));
		Shoot();
	}

    private void CreateAttackVisualization(Vector3 target)
    {
        NavMeshHit hit;
        if(NavMesh.SamplePosition(target, out hit, 5f, NavMesh.AllAreas))
            Instantiate(attackVisualization, hit.position, attackVisualization.transform.rotation);
        else
            Instantiate(attackVisualization, target, attackVisualization.transform.rotation);
    }

	protected override void Shoot() {
		if (launched){

			// This is for the scaling of the rocket (scaling is needed to avoid boarding with the enemy).
			if (transform.localScale.x <= 2f){
				float deltaScaleX = transform.localScale.x;
				float deltaScaleY = transform.localScale.y;
				float deltaScaleZ = transform.localScale.z;
				transform.localScale = new Vector3(deltaScaleX + 2f * Time.deltaTime,deltaScaleY + 2f * Time.deltaTime,deltaScaleZ + 2f * Time.deltaTime);
			} else {
				transform.localScale = new Vector3(2f,2f,2f);
			}

			Vector3 targPos = target;
			if (firstTime){
				// Applying midpoint formula.
				deltaX = (transform.position.x + targPos.x) / 2f;
				deltaZ = (transform.position.z + targPos.z) / 2f;
			}

			if (!headingTarget && transform.position.y <= height){

                // Latching the actual sensitivity.
				actSensitivity = sensitivity;
				
                // It isn't heading the first time anymore.
				firstTime = false;

                // Set the Target Position to the middlepoint of the ballistic curve.
				targPos.x = deltaX;
				targPos.z = deltaZ;

                // Set the Target height to the predefined height.
				targPos.y = height;
			} else {
                // Set the Target Position to the actual target.
				targPos = target;
				headingTarget = true;
			}

			targetPos = targPos;

			Vector3 relativePos  = targPos - transform.position;
			Quaternion rotation  = Quaternion.LookRotation(relativePos);
			
            // Set rotation of the rocket.
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, actSensitivity * Time.deltaTime );
		
			transform.Translate(0,0,bulletSpeed * Time.deltaTime,Space.Self);
		}
	}
	
    /// <summary>
    /// Handles the explosion, damage taking, soundplaying, etc. of the Rocket.
    /// </summary>
    /// <param name="collider">Colliding Object.</param>
	void OnTriggerEnter(Collider collider)
    {
		if(!alreadyTriggered && collider.tag == "Terrain")
        {
            alreadyTriggered = true;    // Ensure that the OnTrigger routine is only triggered once

			// Deactivate mesh renderer.
			MeshRenderer meshRenderer = transform.GetComponentInChildren<MeshRenderer> ();
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

				// if the projectile hits a player, then play the explosion particle without the stain
				if(collider.tag == "Terrain")
					SpawnDeathParticle(new Vector3(transform.position.x, transform.position.y + 0.075f, transform.position.z));
				else
					SpawnAirDeathParticle(transform.position);

                // Camera Shake
                CameraManager.CameraReference.ShakeOnce();

            }
            //Destroy(this.gameObject, 0.1f);
            StartCoroutine(DestroyProjectileAfterTime(0.1f));
        }
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
	/// Spawns the death particles.
	/// </summary>
	private void SpawnAirDeathParticle(Vector3 position)
	{
		GameObject particle = Instantiate(midAirExplosionParticlePrefab) as GameObject;
		particle.transform.position = position;
	}

	/// <summary>
	/// Draws a wiresphere for debug reasons.
	/// </summary>
	void OnDrawGizmos(){
		if (drawDamageRadiusGizmo){
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, damageRadius);
		}
		if (drawMidpointGizmo){
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(targetPos, damageRadius);
		}
	}
}
