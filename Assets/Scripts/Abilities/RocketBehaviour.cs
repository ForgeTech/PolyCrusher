using UnityEngine;
using System.Collections;

public class RocketBehaviour : MonoBehaviour {

	// Rocket which will be launched.
	[SerializeField]
	private float lifeTime = 4f;

	private int damage = 75;

	// Vector of the target which the missle must approach.
	private Vector3 target;

	// Compute after the missle is launched (if it is true then start the method in fixedupdate().
	private bool launched = false;

	// Defines if the missle is targeting the actual target on the floor, else it is heading 
	// to the airpoint.
	private bool headingTarget = false;

	[SerializeField]
	private AudioClip explosionSound;

	// only play once
	private bool playExplode = true;

	// Particle prefab
	[SerializeField]
	private GameObject explosionParticle;

	// The owner of the projectile.
	protected MonoBehaviour ownerScript;

	// Area of effect
	[SerializeField]
	private float damageRadius = 1f;

	// Tweak to adjust top speed.
	[SerializeField]
	private float sensitivity = 0.1f;
	
	// Velocity of the projectile
	[SerializeField]
	protected float bulletSpeed = 50.0f;
	
	// Height which the projectile shoud achieve.
	[SerializeField]
	private float height = 10.0f; 
	
	// true if the code is executed the first time
	private bool firstTime = true;
	
	// Actual sensitivity for the SLerp
	private float actSensitivity;

	private SphereCollider sphereCollider;

	private float xDiff;

	private float zDiff;

	/// <summary>
	/// Gets or sets the owner script.
	/// </summary>
	public MonoBehaviour OwnerScript
	{
		get { return this.ownerScript; }
		set { this.ownerScript = value; }
	}


	/*
    protected override void Start()
    {
        base.Start();
    }
*/
	public void Launch(Vector3 target) {
		this.target = target;

		//this.height = height;
		launched = true;

        //getting the sphere collider of the game object
		sphereCollider = transform.GetComponent<SphereCollider>();

        //setting it to radius = 1f
		sphereCollider.radius = 1f;

        //distance between the position of the transform and the target
		xDiff = Mathf.Abs (transform.position.x - target.x);
		zDiff = Mathf.Abs (transform.position.z - target.z);

        //rotate it to the current targets position (let it head towards it)
		transform.LookAt(new Vector3(target.x,height + 1f,target.z));

        //destroy it after lifetime
		Destroy(this.gameObject, lifeTime);

	}

	public void FixedUpdate() {
		if (launched){

			/*
			// This is for the scaling of the rocclet (scaling is needed to avoid boarding with the enemy)
			if (transform.localScale.x <= 1f){
				float deltaScaleX = transform.localScale.x;
				float deltaScaleY = transform.localScale.y;
				float deltaScaleZ = transform.localScale.z;
				transform.localScale = new Vector3(deltaScaleX + 10f * Time.deltaTime,deltaScaleY + 10f * Time.deltaTime,deltaScaleZ + 10f * Time.deltaTime);
			} else {
				transform.localScale = new Vector3(1f,1f,1f);
			}
			*/
			transform.localScale = new Vector3(1f,1f,1f);
			//sphereCollider.radius = 0.1f;

			Vector3 targPos = target;

			float deltaX;
			float deltaZ;

			deltaX = transform.position.x - targPos.x;
			deltaZ = transform.position.z - targPos.z;

			// If deltax and deltaz are less than the half of the difference transform.position.x - target.x, then head to the ground target
			if (!headingTarget && Mathf.Abs (transform.position.x - target.x)  > xDiff / 1.25f && Mathf.Abs (transform.position.z - target.z) > zDiff / 1.25f ){
				targPos.y = height ;
				if (!firstTime){
					actSensitivity = sensitivity * 4 / ((new Vector3(deltaX, 0, deltaZ).magnitude) + 5f);
				}
				firstTime = false;
			} else {
				targPos = target;
				headingTarget = true;
			}

			targPos.x += deltaX / 10f;
			targPos.z += deltaZ / 10f;

			Vector3 relativePos  = targPos - transform.position;
			Quaternion rotation  = Quaternion.LookRotation(relativePos);
			
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, actSensitivity * Time.deltaTime );
			transform.Translate(0,0,bulletSpeed * Time.deltaTime,Space.Self);

		}
	}

	void OnTriggerEnter(Collider collider){

        //if it collides with the terrain -> let it explode
		if(collider.tag == "Terrain"){

            //spawns the explosion particles
			SpawnExplosionParticle(new Vector3(transform.position.x,  transform.position.y - 0.9f, transform.position.z));

            //getting the component of the transform (sphere collider)
			SphereCollider sphereCollider = transform.GetComponent<SphereCollider>();

            //set the radius to the damage radius
            sphereCollider.radius = damageRadius;

            //get the mesh renderer of the transform
			MeshRenderer meshRenderer =transform.GetComponentInChildren<MeshRenderer>();
            //and disable it
            meshRenderer.enabled = false;
            //play the explosion sound only once
			if (playExplode){
				if (explosionSound != null){
					SoundManager.SoundManagerInstance.Play(explosionSound, transform.position);
					playExplode = false;
				}
			}

            //destroy it after the lifetime to prevent the sound from an interuption
			Destroy(this.gameObject, 2f);
		}
	
		if (collider.tag == "Enemy")
		{
			MonoBehaviour m = collider.gameObject.GetComponent<MonoBehaviour>();
			if (m != null && m is IDamageable)
			{
				((IDamageable)m).TakeDamage(damage, this.ownerScript);
			}
		}
	}
	
	protected void SpawnExplosionParticle(Vector3 position)
	{
		GameObject particle = Instantiate(explosionParticle);
		particle.transform.position = position;

	}
}

