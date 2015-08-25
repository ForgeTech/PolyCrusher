using UnityEngine;
using System.Collections;

public class PowerUpSpawn : MonoBehaviour {


	// The diagonally offset of the spawnpoint for the powerUp
	[SerializeField]
	private float spawnOffset = 5f;

	 // If true, then draw the wirecube
	[SerializeField]
	private bool drawGizmo = true;

	// Array of the spawn positions
	// Initialize array with the positions
	private Vector3[] positions;

	// Maximum of players in which are allowed for the collectible spawn
	private int maxPlayerCount = 4;
	
	public void spawnPowerUps(GameObject[] powerUps, int playerCount){
	
		float positionOffset = spawnOffset / 2;

		positions = new Vector3[4]{
			new Vector3 (transform.position.x + positionOffset, 1f, transform.position.z + positionOffset),
			new Vector3 (transform.position.x - positionOffset, 1f, transform.position.z + positionOffset),
			new Vector3 (transform.position.x + positionOffset, 1f, transform.position.z - positionOffset),
			new Vector3 (transform.position.x - positionOffset, 1f, transform.position.z - positionOffset),
		};

		if (playerCount > maxPlayerCount){
			playerCount = maxPlayerCount;
			//Debug.Log("Collecitble Manager: playerCount set to the maximum of " + maxPlayerCount + " players!");
		}

		for (int i = 0; i < playerCount; i++){
			int randomPowerUpInt = Random.Range(0, powerUps.Length);
			Instantiate(powerUps[randomPowerUpInt].gameObject,positions[i], transform.rotation);
		}
	}

	 void OnDrawGizmos(){
		if (drawGizmo){
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(transform.position, new Vector3(spawnOffset, 0.1f, spawnOffset));
		}
		
	 }
	
}
