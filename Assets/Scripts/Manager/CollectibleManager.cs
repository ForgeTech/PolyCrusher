using UnityEngine;
using System.Collections;

public class CollectibleManager : MonoBehaviour {
	
	// The spawn points of the enemies.
	protected GameObject[] PowerUpSpawnPoints;
	
	// Array of the powerUps
	[SerializeField]
	protected GameObject[] PowerUps;

	// Rate of the fountain particle system
	[SerializeField]
	protected float FountainEmissionRate = 80F;

	// Rate of the pond particle system
	[SerializeField]
	protected float PondEmissionRate = 15F;

    // Particle burst for the pissing pete signs.
    [SerializeField]
    protected GameObject signParticleBurst;
	
	// Number of active PowerUps
	private int activePowerUpsCount = 0;
	
	// Actual spawn point
	private int actualSpawnpoint = 0;
	
	// Number of active players
	private int playerCount;

	// True if it's the first call
	private bool firstCall = true;

    //the signs for the active power up statue
    protected GameObject[] PowerUpSigns;

	void Awake () {
		
		// Search for powerUp spawn points and signs.
		PowerUpSpawnPoints = GameObject.FindGameObjectsWithTag("PowerUpSpawn");
        PowerUpSigns = GameObject.FindGameObjectsWithTag("PowerUpSign");

        // Get the number of the actual active Players
        playerCount = PlayerManager.PlayerCount;
		
		if (playerCount <= 0) {
			playerCount = 1;
		}
		
		// Register event methods.
		PowerUpItem.PowerUpCollected += Collected;
		PowerUpItem.PowerUpCollected += checkSpawn;
		
		if (PowerUpSpawnPoints == null || PowerUpSpawnPoints.Length == 0){
			PowerUpSpawnPoints = null;
		}
	}

	void Start(){
		StartSpawn();
	}

	protected void Collected(){
		activePowerUpsCount--;
	}
	
	protected void checkSpawn(){
		if (PowerUpSpawnPoints != null){
			if (activePowerUpsCount <= 0){

				// Get the number of the actual active Players
				playerCount = PlayerManager.PlayerCount;

				// If there are no players, there should spawn a powerup anyway
				if (playerCount <= 0) {
					playerCount = 1;
				}

				// Exclude the actual spawnpoint to prohibit that the powerUps spawn at the same position
				int[] validSpawns = new int[PowerUpSpawnPoints.Length - 1];
				int validSpawnPos = 0;
				for (int i = 0; i < PowerUpSpawnPoints.Length; i++){
					if (i != actualSpawnpoint){
						validSpawns[validSpawnPos] = i;
						validSpawnPos++;
					}
				}

				foreach (GameObject spawns in PowerUpSpawnPoints){
					// set particle emisiionrate to 0.0F
					ParticleSystem[] particleSystems = spawns.GetComponentsInChildren<ParticleSystem>();
					foreach(ParticleSystem particles in particleSystems){
						particles.emissionRate = 0.0F;
					}
				}

				int spawnInt = 0;

				if (PowerUpSpawnPoints.Length - 1 <= 0){
					validSpawns = new int[PowerUpSpawnPoints.Length];
					validSpawns[0] = 0;
				} else {
					spawnInt = validSpawns[(Random.Range(0, validSpawns.Length))];
				}

				if (firstCall){
					spawnInt = 0;
					firstCall = false;
				}

				// Activate emission of the particle effect from the active powerup spawnpoint
				ParticleSystem FountainParticleEffect = PowerUpSpawnPoints[spawnInt].transform.FindChild("fountain").GetComponent<ParticleSystem>();
				FountainParticleEffect.emissionRate = FountainEmissionRate;
				ParticleSystem PondParticleEffect = PowerUpSpawnPoints[spawnInt].transform.FindChild("pond").GetComponent<ParticleSystem>();
				PondParticleEffect.emissionRate = PondEmissionRate;

				PowerUpSpawnPoints[spawnInt].GetComponent<PowerUpSpawn>().spawnPowerUps(PowerUps, playerCount);
				actualSpawnpoint = spawnInt;
				activePowerUpsCount = playerCount;

                if(PowerUpSigns != null)
                {
                    for(int i = 0; i < PowerUpSigns.Length; i++)
                    {
                        Vector3 targetPosition = PowerUpSpawnPoints[actualSpawnpoint].transform.position;
                        Quaternion neededRotation = Quaternion.LookRotation(targetPosition - PowerUpSigns[i].transform.position);
                        LeanTween.rotate(PowerUpSigns[i], neededRotation.eulerAngles, 0.3f).setEase(LeanTweenType.easeOutSine);
                    }
                }
			}
		}
	}
	// Wait some time to play safe that playercounter will be set to the actual active player at start of the game
	protected IEnumerator WaitUntilFirstPlayerSpawn()
	{
		yield return new WaitForSeconds(0.1f);
		checkSpawn();
	} 
	public void StartSpawn(){
		StartCoroutine("WaitUntilFirstPlayerSpawn");
	}
}