using UnityEngine;
using System.Collections;

public class AbilityRocket : Ability {

	/*

	// Amount of damage which the rocket deals
	[SerializeField]
	protected int damage = 75;

	// Damageradius of the rocket.
	[SerializeField]
	protected float damageRadius = 1;

	// Multiply range with the direction.
	[SerializeField]
	private float mulRange = 10;

	// Angle of the rocket diffuse.
	[SerializeField]
	private float diffuse = 15.0f;

	// Angular offset of the rockets.
	[SerializeField]
	private float angularOffset = 15f;


	// Height of the launched rocket.
	[SerializeField]
	protected float height = 8f;
*/



	// amount of Rockets which are launched.
	[SerializeField]
	private float numberOfRockets = 5;
		
	// Timefactor after the next rocket is launched.
	[SerializeField]
	private float launchTimeFactor = 0.25f;

	// Range factor of the rocket launch range.
	[SerializeField]
	private float range = 5f;

	// Spawn points of the bullets.
	protected Transform[] rocketSpawns;
	
	// Boolean for the spawn point alteration.
	private bool alternateSpawn = true;



	// Rocket which will be launched.
	[SerializeField]
	private GameObject rocket;

	protected override void Start()
	{
		base.Start();
		
		// Get rocket spawns.
		rocketSpawns = transform.GetComponentsInChildren<Transform>();
	}

	public override void Use()
	{
		if (useIsAllowed)
		{
		    base.Use();

			// assign the actual transform to make it editable (Unity.Engine.transform is read only)
			Transform spawn = gameObject.transform;

			for(int i = 0;i < numberOfRockets; i++){
				// Alternate the spawnpoint of the rocket bikini
				if (alternateSpawn){
					spawn = rocketSpawns[1];
				} else {
					spawn = rocketSpawns[2];
				}
				alternateSpawn = !alternateSpawn;
				StartCoroutine(WaitForNextRocket(spawn,i));
			}
			useIsAllowed = false;
			StartCoroutine(WaitForNextAbility());
		}
	}

	public void spawnRocket(Transform spawn, float range) {
		// 0.75f*gameObject.transform.forward because to avoid colliding with the character
		GameObject spawnedRocket = Instantiate(rocket, spawn.position + 0.75f*gameObject.transform.forward, Quaternion.LookRotation(spawn.forward)) as GameObject;
		ParticleSystem particleSystem = spawnedRocket.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		particleSystem.enableEmission = false;

		Vector3 direction = gameObject.transform.forward * range;
		/* 
		direction = Quaternion.Euler(0, -angularOffset + addRotation, 0) * direction;
		direction  *= (mulRange + addRange);
		*/
		Vector3 targetPosition = gameObject.transform.position + direction;
		// Quick and dirty! Set the destination to a negative value, so it muast not be destroyed and the rocket trail is able to smoothly disappear
		targetPosition.y = -50f;

		RocketBehaviour rocketBehaviour = spawnedRocket.GetComponent<RocketBehaviour>();
        rocketBehaviour.OwnerScript = this.OwnerScript;
		spawnedRocket.gameObject.transform.localScale = new Vector3(0.2f,0.2f,0.2f);
		//Debug.Log (range);
		rocketBehaviour.Launch(targetPosition);
		particleSystem.enableEmission = true;
	}

	protected IEnumerator WaitForNextRocket(Transform spawn, int i)
	{
		yield return new WaitForSeconds(i*launchTimeFactor);
		spawnRocket(spawn, range);
	}
}
