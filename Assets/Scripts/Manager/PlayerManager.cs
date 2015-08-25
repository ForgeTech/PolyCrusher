using UnityEngine;
using System.Collections;

/// <summary>
/// Event handler the join of a player.
/// </summary>
public delegate void PlayerJoinedEventHandler();

/// <summary>
/// Event handler for the death of all players.
/// </summary>
public delegate void AllPlayersDeadEventHandler();

/// <summary>
/// Manages the spawning of the player at the start and during
/// the game. 
/// </summary>
public class PlayerManager : MonoBehaviour
{
    #region Class Members
    //The actual player count.
    protected static int playerCount;

    //The actual player prefix.
    protected string playerPrefix;

    //A random radius value, so the players don't spawn at one point.
    [SerializeField]
    protected float randomSpawnRadius = 10.0f;

    //The start spawnPosition of the players
    [SerializeField]
    private Transform spawnPosition;

    //The player slots of the four players. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlot;

	//The player slots of the four players for the phone. False -> Slot free, True -> Slot filled with player
	private bool[] playerSlotPhone;

    // References to the players -> This array should have a size of 4.
    GameObject[] playerReferences;

    //Container that holds the information about the selected characters
    private LevelStartInformation levelInfo;

    //Eventhandler for player joins
    public static event PlayerJoinedEventHandler PlayerJoinedEventHandler;

    //Eventhandler for the death of all players.
    public static event AllPlayersDeadEventHandler AllPlayersDeadEventHandler;
    #endregion


    #region Properties
    /// <summary>
    /// Gets the actual player count.
    /// </summary>
    public static int PlayerCount
    {
        get { return playerCount; }
    }

    /// <summary>
    /// Gets the actual playerPrefix.
    /// </summary>
    public string PlayerPrefix
    {
        get
        {
            return "P" + playerCount + "_";
        }
    }

    #endregion

    #region Methods

   static PlayerManager()
    {
        playerCount = 0;
    }

    void Awake()
    {
        // Init player reference Array.
        playerReferences = new GameObject[4];

        for (int i = 0; i < playerReferences.Length; i++)
            playerReferences[i] = null;


        // Register events
        BasePlayer.PlayerSpawned += PlayerSpawned;
        BasePlayer.PlayerDied += DecrementPlayerCount;
        BasePlayer.PlayerDied += CheckPlayerLifeStatus;
        LevelEndManager.levelExitEvent += ResetValues;
        
        //Respawn dead players on wave end
        GameManager.WaveEnded += RespawnDeadPlayers;
    }
    
    // Use this for initialization
	void Start () 
    {
        //If there are more spawn points, choose the first one.
        if (spawnPosition == null && GameObject.FindGameObjectsWithTag("PlayerSpawn").Length > 0)
            spawnPosition = GameObject.FindGameObjectsWithTag("PlayerSpawn")[0].transform;

        playerSlot = new bool[4];
        playerSlotPhone = new bool[4];

        levelInfo = GameObject.FindObjectOfType<LevelStartInformation>();
        
        //Assign the used slots
        if (levelInfo != null)
        {
            for (int i = 0; i < playerSlot.Length; i++)
            {
                GameObject prefab;

                if (levelInfo.playerSlot[i] != null)
                {
                    playerSlot[i] = true;
                    string prefabPath = "Player/" + levelInfo.playerSlot[i];
                    string prefix = "P" + (i+1) + "_";
                    prefab = Instantiate(Resources.Load<GameObject>(prefabPath)) as GameObject;
                    prefab.GetComponent<NavMeshAgent>().enabled = false;
                    prefab.transform.position = spawnPosition.position;
                    prefab.gameObject.name = "Player" + (playerCount + 1);
                    prefab.GetComponent<BasePlayer>().PlayerPrefix = prefix;
                    prefab.GetComponent<NavMeshAgent>().enabled = true;

                    OnPlayerJoined();
                }
                else
                {
                    playerSlot[i] = false;
                }

                if (levelInfo.phonePlayerSlot[i] != null)
                {
                    playerSlotPhone[i] = true;
                    string prefabPath = "Player/" + levelInfo.phonePlayerSlot[i];
                    prefab = Instantiate(Resources.Load<GameObject>(prefabPath)) as GameObject;
					prefab.GetComponent<NavMeshAgent>().enabled = false;
                    prefab.transform.position = spawnPosition.position;
                    prefab.gameObject.name = "Player" + (playerCount + 1);
                    prefab.GetComponent<BasePlayer>().PhonePlayerSlot = i;
					prefab.GetComponent<NavMeshAgent>().enabled = true;

                    OnPlayerJoined();
                }
                else
                {
                    playerSlotPhone[i] = false;
                }
            }
        }
       


       
	}

    // Update is called once per frame
    void Update() 
    {
        // For testing
        //HandlePlayerJoin();
    }

    /// <summary>
    /// Adds a new player to the game.
    /// </summary>
    private void HandlePlayerJoin()
    {
        GameObject prefab;

		string[] characterArray = new string[4] {"Timeshifter", "Charger", "Fatman", "Birdman"};
		bool[] characterTaken = new bool[4] {false, false, false, false};
		string playerPref = "";
		bool join = false;

		if (playerCount < 4) {

			if (Input.GetAxis ("P1_Join") > 0 && !playerSlot [0]) {
				playerSlot [0] = true;
				playerPref = "P1_";
				join = true;
			} else if (Input.GetAxis ("P2_Join") > 0 && !playerSlot [1]) {
				playerSlot [1] = true;
				playerPref = "P2_";
				join = true;
			} else if (Input.GetAxis ("P3_Join") > 0 && !playerSlot [2]) {
				playerSlot [2] = true;
				playerPref = "P3_";
				join = true;
			} else if (Input.GetAxis ("P4_Join") > 0 && !playerSlot [3]) {
				playerSlot [2] = true;
				playerPref = "P3_";
				join = true;
			}

			if (join) {
				Debug.Log ("Player " + (playerCount + 1) + " joined! (Controller)");

				string takeCharacter = "Birdman";
				
				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if (levelInfo.playerSlot [i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot [i].Equals (characterArray [j])) {
									characterTaken [j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if (levelInfo.phonePlayerSlot [i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot [i].Equals (characterArray [j])) {
									characterTaken [j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < characterTaken.Length; i++) {
						if (characterTaken [i] == false) {
							takeCharacter = characterArray [i];
						}
					}
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if (levelInfo.playerSlotTaken [i] == false) {
							levelInfo.playerSlot [i] = takeCharacter;
							levelInfo.playerSlotTaken [i] = true;
						}
					}
				}
				
				prefab = Instantiate (Resources.Load<GameObject> ("Player/" + takeCharacter)) as GameObject;
				prefab.GetComponent<NavMeshAgent> ().enabled = false;
				prefab.transform.position = GetPositionOfRandomPlayer (true);
				prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer> ().PlayerPrefix = playerPref;
				prefab.GetComponent<NavMeshAgent> ().enabled = true;
				
				OnPlayerJoined ();

				for (int i = 0; i < playerSlot.Length; i++) {
					if (playerSlot[i] == false) {
						playerSlot[i] = true;
						break;
					}
				}
				
			}
		}

    }

	/// <summary>
	/// Adds a new phone player to the game.
	/// </summary>
	public void HandlePhonePlayerJoin(int slot)
	{
		GameObject prefab;
		string[] characterArray = new string[4] {"Timeshifter", "Charger", "Fatman", "Birdman"};
		bool[] characterTaken = new bool[4] {false, false, false, false};
		string playerPref = "";
		bool join = false;
		
		if (playerCount < 4) {
			if (!playerSlotPhone[0]) {
				playerSlotPhone[0] = true;
				join = true;
			} else if (!playerSlotPhone[1]) {
				playerSlotPhone[1] = true;
				join = true;
			} else if (!playerSlotPhone[2]) {
				playerSlotPhone[2] = true;
				join = true;
			} else if (!playerSlotPhone[3]) {
				playerSlotPhone[3] = true;
				join = true;
			}

			if (join) {
				Debug.Log("Player " + (playerCount + 1)  + " joined! (Phone)");

				string takeCharacter = "Birdman";
				
				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if(levelInfo.playerSlot[i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if(levelInfo.phonePlayerSlot[i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < characterTaken.Length; i++) {
						if (characterTaken[i] == false) {
							takeCharacter = characterArray[i];
						}
					}
					
					for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
						if (levelInfo.phonePlayerSlotTaken[j] == false) {
							levelInfo.phonePlayerSlot[j] = takeCharacter;
							levelInfo.phonePlayerSlotTaken[j] = true;
						}
					}
				}
				
				prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
				prefab.GetComponent<NavMeshAgent>().enabled = false;
				prefab.transform.position = GetPositionOfRandomPlayer(true);
				prefab.GetComponent<NavMeshAgent>().enabled = true;
				prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer>().PhonePlayerSlot = slot;
				
				OnPlayerJoined();

				for (int i = 0; i < playerSlotPhone.Length; i++) {
					if (playerSlotPhone[i] == false) {
						playerSlotPhone[i] = true;
						break;
					}
				}
			}
		}
	}

    /// <summary>
    /// Respawns the dead players to the living players.
    /// </summary>
    public void RespawnDeadPlayers()
    {
        // Loop throug the playerReferences array to check if there are dead players to respawn.
        for (int i = 0; i < playerReferences.Length; i++)
        {
            // Only respawn the player if he was dead.
            if (playerReferences[i] != null && !playerReferences[i].gameObject.activeSelf)
            {
                playerReferences[i].gameObject.SetActive(true);
                playerReferences[i].GetComponent<NavMeshAgent>().enabled = false;
                playerReferences[i].gameObject.transform.position = GetPositionOfRandomPlayer(true);
                playerReferences[i].GetComponent<NavMeshAgent>().enabled = true;
                playerReferences[i].gameObject.GetComponent<BasePlayer>().RevivePlayer();
                //IncrementPlayerCount();
            }
        }
    }

    /// <summary>
    /// Calculates a random position based on the origin position. (Y-Value will not be affected)
    /// </summary>
    /// <param name="origin">Origin position.</param>
    /// <returns></returns>
    private Vector3 CalculateRandomPosition(Vector3 origin)
    {
        Vector3 position = new Vector3(origin.x + Random.Range(0f, randomSpawnRadius), origin.y , origin.z + Random.Range(0f, randomSpawnRadius));
        return position;
    }

    /// <summary>
    /// Return the position of a random player.
    /// </summary>
    /// <returns>Random player position</returns>
    private Vector3 GetPositionOfRandomPlayer()
    {
        //Player array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        return players[Random.Range(0, players.Length)].transform.position;
    }

    /// <summary>
    /// Return the position of a random player.
    /// </summary>
    /// <param name="radius">If true: return the random position with a offset</param>
    /// <returns>Random player position with offset</returns>
    private Vector3 GetPositionOfRandomPlayer(bool radius)
    {
        if (!radius)
            return GetPositionOfRandomPlayer();
        
        // Return value
        Vector3 position = Vector3.zero;

        //Player array
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 1)
        {
            NavMeshHit hit;

            // Calculate position with random pos.
            position = players[Random.Range(0, players.Length - 1)].transform.position;
            position.x += Random.Range(-randomSpawnRadius, randomSpawnRadius);
            position.z += Random.Range(-randomSpawnRadius, randomSpawnRadius);

            // Sample position on navmesh.
            NavMesh.SamplePosition(position, out hit, randomSpawnRadius, NavMesh.AllAreas);

            position = hit.position;
        }
        else
            position = spawnPosition.position;

        return position;
    }

    /// <summary>
    /// Should be called if player joins.
    /// </summary>
    protected virtual void OnPlayerJoined()
    {
        if (PlayerJoinedEventHandler != null)
            PlayerJoinedEventHandler();
    }

    /// <summary>
    /// Should be called if all players are dead.
    /// </summary>
    protected virtual void OnAllPlayersDied()
    {
        if (AllPlayersDeadEventHandler != null)
            AllPlayersDeadEventHandler();
    }

    /// <summary>
    /// Method for the delegate, if a player is activated.
    /// </summary>
    protected virtual void PlayerSpawned(BasePlayer p)
    {
        IncrementPlayerCount();

        // Fill the playerReferences array.

        for (int i = 0; i < playerReferences.Length; i++)
        {
            if (playerReferences[i] == null)
            {
                playerReferences[i] = p.gameObject;
                break;
            }

            if (playerReferences[i] == p.gameObject)
                break;
        }
    }

    /// <summary>
    /// Increments the playercount.
    /// </summary>
    protected virtual void IncrementPlayerCount()
    {
        playerCount++;
        Debug.Log("PlayerManager: Playercount - " + PlayerCount);
    }

    /// <summary>
    /// Decrements the playercount.
    /// </summary>
    protected virtual void DecrementPlayerCount()
    {
        if (playerCount - 1 >= 0)
        {
            playerCount--;
            Debug.Log("PlayerManager: Playercount - " + PlayerCount);
        }
    }

    /// <summary>
    /// Checks if all players are dead.
    /// If all players are dead, the "AllPlayerDead"-event will be fired.
    /// </summary>
    protected virtual void CheckPlayerLifeStatus()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Fire event if all players died.
        if (players.Length == 0)
        {
            OnAllPlayersDied();
            Debug.Log("PlayerManager: All Players are dead!");
        }
    }

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected void ResetValues()
    {
        PlayerJoinedEventHandler = null;
        AllPlayersDeadEventHandler = null;
    }
    #endregion
}

/*
 * 	
			if (Input.GetAxis("P2_Join") > 0 && !playerSlot[1])
			{
				Debug.Log("Player " + (playerCount + 1) + " joined! (Controller)");
				playerSlot[1] = true;

				string takeCharacter = "Birdman";
				
				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if(levelInfo.playerSlot[i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if(levelInfo.phonePlayerSlot[i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					int ix;
					
					for (ix = 0; ix < characterTaken.Length; ix++) {
						if (characterTaken[ix] == false) {
							takeCharacter = characterArray[ix];
						}
					}
					
					for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
						if (levelInfo.playerSlotTaken[j] == false) {
							levelInfo.playerSlot[j] = takeCharacter;
							levelInfo.playerSlotTaken[j] = true;
						}
					}
				}
				
				prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
                prefab.GetComponent<NavMeshAgent>().enabled = false;
                prefab.transform.position = GetPositionOfRandomPlayer(true);
				prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer>().PlayerPrefix = "P2_";
                prefab.GetComponent<NavMeshAgent>().enabled = true;

                OnPlayerJoined();
			}
			*/

/*	/// <summary>
	/// Adds a new phone player to the game.
	/// </summary>
	public void HandlePhonePlayerJoin(int slot)
	{
		GameObject prefab;
		bool joined = false;

		string[] characterArray = new string[4] {"Timeshifter", "Charger", "Fatman", "Birdman"};
		bool[] characterTaken = new bool[4] {false, false, false, false};

		if (playerCount < 4) {
			if (!joined && !playerSlotPhone[0])
			{
				Debug.Log("Player " + (playerCount + 1)  + " joined! (Phone)");
				playerSlotPhone[0] = true;
				joined = true;

				string takeCharacter = "Birdman";

				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if(levelInfo.playerSlot[i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if(levelInfo.phonePlayerSlot[i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					int ix;
					
					for (ix = 0; ix < characterTaken.Length; ix++) {
						if (characterTaken[ix] == false) {
							takeCharacter = characterArray[ix];
						}
					}
					
					for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
						if (levelInfo.phonePlayerSlotTaken[j] == false) {
							levelInfo.phonePlayerSlot[j] = takeCharacter;
							levelInfo.phonePlayerSlotTaken[j] = true;
						}
					}
				}
				
				prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
                prefab.GetComponent<NavMeshAgent>().enabled = false;
                prefab.transform.position = GetPositionOfRandomPlayer(true);
                prefab.GetComponent<NavMeshAgent>().enabled = true;
                prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer>().PhonePlayerSlot = slot;
				

				OnPlayerJoined();
			}
			
			if (!joined && !playerSlotPhone[1])
			{
				Debug.Log("Player " + (playerCount + 1) + " joined! (Phone)");
				playerSlotPhone[1] = true;
				joined = true;

				string takeCharacter = "Birdman";
				
				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if(levelInfo.playerSlot[i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if(levelInfo.phonePlayerSlot[i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					int ix;
					
					for (ix = 0; ix < characterTaken.Length; ix++) {
						if (characterTaken[ix] == false) {
							takeCharacter = characterArray[ix];
						}
					}
					
					for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
						if (levelInfo.phonePlayerSlotTaken[j] == false) {
							levelInfo.phonePlayerSlot[j] = takeCharacter;
							levelInfo.phonePlayerSlotTaken[j] = true;
						}
					}
				}
				
				prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
                prefab.GetComponent<NavMeshAgent>().enabled = false;
                prefab.transform.position = GetPositionOfRandomPlayer(true);
                prefab.GetComponent<NavMeshAgent>().enabled = true;
                prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer>().PhonePlayerSlot = slot;
				
				OnPlayerJoined();
			}
			
			if (!joined && !playerSlotPhone[2])
			{
				Debug.Log("Player " + (playerCount + 1) + " joined! (Phone)");
				playerSlotPhone[2] = true;
				joined = true;

				string takeCharacter = "Birdman";
				
				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if(levelInfo.playerSlot[i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if(levelInfo.phonePlayerSlot[i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					int ix;
					
					for (ix = 0; ix < characterTaken.Length; ix++) {
						if (characterTaken[ix] == false) {
							takeCharacter = characterArray[ix];
						}
					}
					
					for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
						if (levelInfo.phonePlayerSlotTaken[j] == false) {
							levelInfo.phonePlayerSlot[j] = takeCharacter;
							levelInfo.phonePlayerSlotTaken[j] = true;
						}
					}
				}
				
				prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
                prefab.GetComponent<NavMeshAgent>().enabled = false;
                prefab.transform.position = GetPositionOfRandomPlayer(true);
                prefab.GetComponent<NavMeshAgent>().enabled = true;
                prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer>().PhonePlayerSlot = slot;
				
				OnPlayerJoined();
			}
			
			if (!joined && !playerSlotPhone[3])
			{
				Debug.Log("Player " + (playerCount + 1) + " joined! (Phone)");
				playerSlotPhone[3] = true;
				joined = true;

				string takeCharacter = "Birdman";
				
				if (levelInfo != null) {
					
					for (int i = 0; i < levelInfo.playerSlot.Length; i++) {
						if(levelInfo.playerSlot[i] != null) {
							for (int j = 0; j < levelInfo.playerSlot.Length; j++) {
								if (levelInfo.playerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					for (int i = 0; i < levelInfo.phonePlayerSlot.Length; i++) {
						if(levelInfo.phonePlayerSlot[i] != null) {
							for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
								if (levelInfo.phonePlayerSlot[i].Equals(characterArray[j])) {
									characterTaken[j] = true;
								}
							}
						}
					}
					
					int ix;
					
					for (ix = 0; ix < characterTaken.Length; ix++) {
						if (characterTaken[ix] == false) {
							takeCharacter = characterArray[ix];
						}
					}
					
					for (int j = 0; j < levelInfo.phonePlayerSlot.Length; j++) {
						if (levelInfo.phonePlayerSlotTaken[j] == false) {
							levelInfo.phonePlayerSlot[j] = takeCharacter;
							levelInfo.phonePlayerSlotTaken[j] = true;
						}
					}
				}
				
				prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
                prefab.GetComponent<NavMeshAgent>().enabled = false;
                prefab.transform.position = GetPositionOfRandomPlayer(true);
                prefab.GetComponent<NavMeshAgent>().enabled = true;
                prefab.gameObject.name = "Player" + (playerCount + 1);
				prefab.GetComponent<BasePlayer>().PhonePlayerSlot = slot;
				
				OnPlayerJoined();

			}
		}
		

	}
 */
