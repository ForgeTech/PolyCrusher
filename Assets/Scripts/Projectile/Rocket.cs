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


	public void Shoot(Vector3 target) {
		this.target = new Vector3(target.x, target.y, target.z);
		launched = true;
		SphereCollider sphereCollider = transform.GetComponent<SphereCollider>();
		sphereCollider.radius = 0.1f;
		Destroy (gameObject,lifeTime);
		Shoot();
	}
	
	protected override void Shoot() {
		if (launched){

			// This is for the scaling of the rocclet (scaling is needed to avoid boarding with the enemy)
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
				// Applying midpoint formula
				deltaX = (transform.position.x + targPos.x) / 2f;
				deltaZ = (transform.position.z + targPos.z) / 2f;
			}

			if (!headingTarget && transform.position.y <= height){

				actSensitivity = sensitivity;
				//actSensitivity = sensitivity  / ((new Vector3(deltaX, 0, deltaZ).magnitude));
				//actSensitivity = sensitivity * 4 / ((new Vector3(deltaX, 0, deltaZ).magnitude) + 5f);
				
				firstTime = false;

				targPos.x = deltaX;
				targPos.z = deltaZ;

				targPos.y = height;
			} else {
				targPos = target;
				headingTarget = true;
			}

			targetPos = targPos;

			Vector3 relativePos  = targPos - transform.position;
			Quaternion rotation  = Quaternion.LookRotation(relativePos);
			
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, actSensitivity * Time.deltaTime );
		
			transform.Translate(0,0,bulletSpeed * Time.deltaTime,Space.Self);
		}
	}
	
	void OnTriggerEnter(Collider collider){
		if(collider.tag == "Terrain" || collider.tag == "Player"){
			//SphereCollider sphereCollider = transform.GetComponent<SphereCollider>();
			//sphereCollider.radius = damageRadius;

			// Deactivate mesh renderer.
			MeshRenderer meshRenderer =transform.GetComponentInChildren<MeshRenderer>();
			meshRenderer.enabled = false;

			// only calling the explosionsound method once
			if (playExplode){
				if (explosionSound != null){
					SoundManager.SoundManagerInstance.Play(explosionSound, transform.position);
					playExplode = false;
				}

				Collider[] collidingObjects = Physics.OverlapSphere(transform.position, damageRadius);
				
				foreach (Collider objects in collidingObjects){
					if (objects.tag == "Player"){
						MonoBehaviour m = objects.gameObject.GetComponent<MonoBehaviour>();
						if (m != null && m is IDamageable)
						{
							((IDamageable)m).TakeDamage(damage, this.OwnerScript);
						}
					}
				}

				// if the projectile hits a player, then play the explosion particle without the stain
				if(collider.tag == "Terrain"){
					SpawnDeathParticle(new Vector3(transform.position.x, transform.position.y + 0.075f, transform.position.z));
				} else {
					SpawnAirDeathParticle(transform.position);
				}

                // Camera Shake
                CameraManager.CameraReference.ShakeOnce();

            }
			Destroy(this.gameObject, 0.1f);
		}
	}
	
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
