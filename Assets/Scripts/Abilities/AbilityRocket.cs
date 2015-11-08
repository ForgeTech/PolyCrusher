using UnityEngine;
using System.Collections;

/// <summary>
/// The rocket ability script, preferential for the Fatman, dervies from Ability.
/// </summary>
public class AbilityRocket : Ability {

    #region Class Members

    [Header("Rocket Settings")]
    // Amount of Rockets which are launched.
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

    #endregion

    #region Methods

    /// <summary>
    /// Overrides start method from the derived class "Ability".
    /// This method will be executed on start.
    /// </summary>
    protected override void Start()
	{
        // Access to the Start() of the derived class with base, calls Start()
        base.Start();
		
		// Get rocket spawnpoints.
		rocketSpawns = transform.GetComponentsInChildren<Transform>();
	}

    /// <summary>
    /// Method for using the sepcial ability
    /// This method will be executed on start.
    /// </summary>
    public override void Use()
	{
		if (useIsAllowed)
		{
            // Access to the Use() of the derived class with base, calls Use()
            base.Use();

			// Assign the actual transform to make it editable (Unity.Engine.transform is read only)
			Transform spawn = gameObject.transform;

            // Spawn the rockets
			for(int i = 0;i < numberOfRockets; i++){
				// Alternate the spawnpoint of the rocket bikini
				if (alternateSpawn){
					spawn = rocketSpawns[1];
				} else {
					spawn = rocketSpawns[2];
				}
				alternateSpawn = !alternateSpawn;

                // Wait a little time before the next rocket is goint to be fired
				StartCoroutine(WaitForNextRocket(spawn,i));
			}

            // Sets useAllowed to false so the player won't be able to spam the special ability
			useIsAllowed = false;

            // Start waiting coroutine
			StartCoroutine(WaitForNextAbility());
		}
	}

    /// <summary>
    /// Method for spawning the rockets.
    /// </summary>
    /// <param name="spawn">The actual spawn Transform(either left or right bikini side).</param>
    /// <param name="range">Actual range parameter.</param>
    /// <returns></returns>
    public void spawnRocket(Transform spawn, float range) {
		// 0.75f*gameObject.transform.forward because to avoid colliding with the character
        GameObject spawnedRocket = ObjectsPool.Spawn(rocket, spawn.position + 0.75f * gameObject.transform.forward, Quaternion.LookRotation(spawn.forward));

        // Gets the particle system from the parent game object
        ParticleSystem particleSystem = spawnedRocket.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
        // The emission of the particle system is disabled because of the default rotation
        particleSystem.enableEmission = false;

        // Set the looking direction of the rocket to the target
		Vector3 direction = gameObject.transform.forward * range;
		
        /* 
		direction = Quaternion.Euler(0, -angularOffset + addRotation, 0) * direction;
		direction  *= (mulRange + addRange);
		*/

        // Set the target position of the rocket
		Vector3 targetPosition = gameObject.transform.position + direction;
		// Quick and dirty! Set the destination to a negative value, so it muast not be destroyed and the rocket trail is able to smoothly disappear
		targetPosition.y = -50f;

        // Get the rocketbehaviour component...
		RocketBehaviour rocketBehaviour = spawnedRocket.GetComponent<RocketBehaviour>();
        // And set the ownerscript (for datamining)
        rocketBehaviour.OwnerScript = this.OwnerScript;
        // Scale the size of the rocket to one fith of the original size
		spawnedRocket.gameObject.transform.localScale = new Vector3(0.2f,0.2f,0.2f);
        // Call Launch from the attached rocketbehaviour script
		rocketBehaviour.Launch(targetPosition);
        // The rocket looks into the right direction so we can enable the emission of the particle system
		particleSystem.enableEmission = true;
	}

    /// <summary>
    /// Wait before the next rocket will be shot.
    /// </summary>
    /// <param name="spawn">The actual spawn Transform(either left or right bikini side).</param>
    /// <param name="i">Describes the launch time offset.</param>
    /// <returns></returns>
    protected IEnumerator WaitForNextRocket(Transform spawn, int i)
	{
		yield return new WaitForSeconds(i*launchTimeFactor);
		spawnRocket(spawn, range);
	}

    #endregion
}
