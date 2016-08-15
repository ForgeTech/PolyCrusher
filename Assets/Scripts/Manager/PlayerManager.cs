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
    [Header("Spawning option")]
    [SerializeField]
    [Tooltip("Random player radius (for respawning).")]
    protected float randomSpawnRadius = 10.0f;

    //The start spawnPosition of the players
    [SerializeField]
    [Tooltip("Spawn position for the game start.")]
    private Transform spawnPosition;

    [SerializeField]
    [Tooltip("Specifies if a dead player will be respawned after a wave or not.")]
    protected bool respawnPlayersAfterDeath = true;

    //The player slots of the four players. False -> Slot free, True -> Slot filled with player
    private bool[] playerSlot;


    // References to the players -> This array should have a size of 4.
    GameObject[] playerReferences;

    //Container that holds the information about the selected characters
    private PlayerSelectionContainer playerSelectionInformation;

    // The time of a whole game.
    private static TimeUtil playTime = null;

    // Start time for time measurement.
    private float startTime = 0;

    // Signals if the time should be measured or not.
    private bool measureTime;

    //Eventhandler for player joins
    public static event PlayerJoinedEventHandler PlayerJoinedEventHandler;

    //Eventhandler for the death of all players.
    public static event AllPlayersDeadEventHandler AllPlayersDeadEventHandler;
    #endregion


    #region Properties
    /// <summary>
    /// Gets the player count of the actual game session.
    /// Also dead players will be in the count number.
    /// </summary>
    public int PlayerCountInGameSession
    {
        get
        {
            int count = 0;
            for (int i = 0; i < playerSlot.Length; i++)
            {
                if (playerSlot[i])
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Gets all players of the actual game session.
    /// Also dead players will be counted.
    /// </summary>
    public BasePlayer[] PlayersInGameSession
    {
        get
        {
            BasePlayer[] currPlayers = new BasePlayer[PlayerCountInGameSession];
            for (int i = 0, j = 0; i < playerReferences.Length && j < currPlayers.Length; i++)
            {
                if (playerReferences[i] != null)
                {
                    BasePlayer p = playerReferences[i].GetComponent<BasePlayer>();

                    if (p != null)
                    {
                        currPlayers[j] = p;
                        j++;
                    }
                }
            }

            return currPlayers;
        }
    }

    /// <summary>
    /// Gets all player names of the actual game session.
    /// Also dead players will be in the count number.
    /// </summary>
    public string[] PlayerNamesInGameSession
    {
        get
        {
            BasePlayer[] p = PlayersInGameSession;
            string[] playerStrings = new string[p.Length];

            for (int i = 0; i < p.Length; i++)
                playerStrings[i] = p[i].PlayerIdentifier.ToString("g");

            return playerStrings;
        }
    }

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

    /// <summary>
    /// Gets the play time.
    /// </summary>
    public static TimeUtil PlayTime
    {
        get { return PlayerManager.playTime; }
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

        // All players died
        PlayerManager.AllPlayersDeadEventHandler += DisableTimeMeasurement;
    }
    
    // Use this for initialization
	void Start () 
    {
        //If there are more spawn points, choose the first one.
        if (spawnPosition == null && GameObject.FindGameObjectsWithTag("PlayerSpawn").Length > 0)
            spawnPosition = GameObject.FindGameObjectsWithTag("PlayerSpawn")[0].transform;

        playerSlot = new bool[4];
       

        playerSelectionInformation = GameObject.FindObjectOfType<PlayerSelectionContainer>();

        playTime = new TimeUtil();
        startTime = (int)Time.realtimeSinceStartup;
        measureTime = true;

        // start session
        DataCollector.instance.startSession();

        //Assign the used slots
        if (playerSelectionInformation != null)
        {
            for (int i = 0; i < playerSlot.Length; i++)
            {
                GameObject prefab;

                if (playerSelectionInformation.playerActive[i])
                {
                    playerSlot[i] = true;
                    prefab = Instantiate(playerSelectionInformation.playerPrefabs[playerSelectionInformation.playerPrefabIndices[i]]) as GameObject;
                    prefab.GetComponent<NavMeshAgent>().enabled = false;
                    prefab.transform.position = spawnPosition.position;                   
                    prefab.GetComponent<BasePlayer>().InputDevice = playerSelectionInformation.playerInputDevices[i];
                    //prefab.GetComponent<BasePlayer>().PlayerActions = playerSelectionInformation.playerActions[i];
                    prefab.GetComponent<NavMeshAgent>().enabled = true;
                    OnPlayerJoined();
                }
                else
                {
                    playerSlot[i] = false;
                }               
            }
        }
	}

    // Update is called once per frame
    void Update() 
    {
        // For testing
        //HandlePlayerJoin();

        UpdatePlayTime();
    }

    /// <summary>
    /// Calculates the game time.
    /// </summary>
    protected void UpdatePlayTime()
    {
        if (measureTime)
        {
            float endTime = Time.realtimeSinceStartup;
            playTime = TimeUtil.SecondsToTime(endTime - startTime);
        }
    }

    /// <summary>
    /// Disables the time measurement.
    /// </summary>
    protected void DisableTimeMeasurement()
    {
        measureTime = false;
    }

    /// <summary>
    /// Adds a new player to the game.
    /// </summary>
  //  private void HandlePlayerJoin()
  //  {
  //      GameObject prefab;

		//string[] characterArray = new string[4] {"Timeshifter", "Charger", "Fatman", "Birdman"};
		//bool[] characterTaken = new bool[4] {false, false, false, false};
		//string playerPref = "";
		//bool join = false;

		//if (playerCount < 4) {

		//	if (Input.GetAxis ("P1_Join") > 0 && !playerSlot [0]) {
		//		playerSlot [0] = true;
		//		playerPref = "P1_";
		//		join = true;
		//	} else if (Input.GetAxis ("P2_Join") > 0 && !playerSlot [1]) {
		//		playerSlot [1] = true;
		//		playerPref = "P2_";
		//		join = true;
		//	} else if (Input.GetAxis ("P3_Join") > 0 && !playerSlot [2]) {
		//		playerSlot [2] = true;
		//		playerPref = "P3_";
		//		join = true;
		//	} else if (Input.GetAxis ("P4_Join") > 0 && !playerSlot [3]) {
		//		playerSlot [2] = true;
		//		playerPref = "P3_";
		//		join = true;
		//	}

		//	if (join) {
		//		Debug.Log ("Player " + (playerCount + 1) + " joined! (Controller)");

		//		string takeCharacter = "Birdman";
				
		//		if (playerSelectionInformation != null) {
					
		//			for (int i = 0; i < playerSelectionInformation.playerSlot.Length; i++) {
		//				if (playerSelectionInformation.playerSlot [i] != null) {
		//					for (int j = 0; j < playerSelectionInformation.playerSlot.Length; j++) {
		//						if (playerSelectionInformation.playerSlot [i].Equals (characterArray [j])) {
		//							characterTaken [j] = true;
		//						}
		//					}
		//				}
		//			}
					
		//			for (int i = 0; i < playerSelectionInformation.phonePlayerSlot.Length; i++) {
		//				if (playerSelectionInformation.phonePlayerSlot [i] != null) {
		//					for (int j = 0; j < playerSelectionInformation.phonePlayerSlot.Length; j++) {
		//						if (playerSelectionInformation.phonePlayerSlot [i].Equals (characterArray [j])) {
		//							characterTaken [j] = true;
		//						}
		//					}
		//				}
		//			}
					
		//			for (int i = 0; i < characterTaken.Length; i++) {
		//				if (characterTaken [i] == false) {
		//					takeCharacter = characterArray [i];
		//				}
		//			}
					
		//			for (int i = 0; i < playerSelectionInformation.playerSlot.Length; i++) {
		//				if (playerSelectionInformation.playerSlotTaken [i] == false) {
		//					playerSelectionInformation.playerSlot [i] = takeCharacter;
		//					playerSelectionInformation.playerSlotTaken [i] = true;
		//				}
		//			}
		//		}
				
		//		prefab = Instantiate (Resources.Load<GameObject> ("Player/" + takeCharacter)) as GameObject;
		//		prefab.GetComponent<NavMeshAgent> ().enabled = false;
		//		prefab.transform.position = GetPositionOfRandomPlayer (true);
		//		prefab.gameObject.name = "Player" + (playerCount + 1);
		//		prefab.GetComponent<BasePlayer> ().PlayerPrefix = playerPref;
		//		prefab.GetComponent<NavMeshAgent> ().enabled = true;
				
		//		OnPlayerJoined ();

		//		for (int i = 0; i < playerSlot.Length; i++) {
		//			if (playerSlot[i] == false) {
		//				playerSlot[i] = true;
		//				break;
		//			}
		//		}
				
		//	}
		//}

  //  }

	/// <summary>
	/// Adds a new phone player to the game.
	/// </summary>
	//public void HandlePhonePlayerJoin(int slot)
	//{
	//	GameObject prefab;
	//	string[] characterArray = new string[4] {"Timeshifter", "Charger", "Fatman", "Birdman"};
	//	bool[] characterTaken = new bool[4] {false, false, false, false};
	//	//string playerPref = "";
	//	bool join = false;
		
	//	if (playerCount < 4) {
	//		if (!playerSlotPhone[0]) {
	//			playerSlotPhone[0] = true;
	//			join = true;
	//		} else if (!playerSlotPhone[1]) {
	//			playerSlotPhone[1] = true;
	//			join = true;
	//		} else if (!playerSlotPhone[2]) {
	//			playerSlotPhone[2] = true;
	//			join = true;
	//		} else if (!playerSlotPhone[3]) {
	//			playerSlotPhone[3] = true;
	//			join = true;
	//		}

	//		if (join) {
	//			Debug.Log("Player " + (playerCount + 1)  + " joined! (Phone)");

	//			string takeCharacter = "Birdman";
				
	//			if (playerSelectionInformation != null) {
					
	//				for (int i = 0; i < playerSelectionInformation.playerSlot.Length; i++) {
	//					if(playerSelectionInformation.playerSlot[i] != null) {
	//						for (int j = 0; j < playerSelectionInformation.playerSlot.Length; j++) {
	//							if (playerSelectionInformation.playerSlot[i].Equals(characterArray[j])) {
	//								characterTaken[j] = true;
	//							}
	//						}
	//					}
	//				}
					
	//				for (int i = 0; i < playerSelectionInformation.phonePlayerSlot.Length; i++) {
	//					if(playerSelectionInformation.phonePlayerSlot[i] != null) {
	//						for (int j = 0; j < playerSelectionInformation.phonePlayerSlot.Length; j++) {
	//							if (playerSelectionInformation.phonePlayerSlot[i].Equals(characterArray[j])) {
	//								characterTaken[j] = true;
	//							}
	//						}
	//					}
	//				}
					
	//				for (int i = 0; i < characterTaken.Length; i++) {
	//					if (characterTaken[i] == false) {
	//						takeCharacter = characterArray[i];
	//					}
	//				}
					
	//				for (int j = 0; j < playerSelectionInformation.phonePlayerSlot.Length; j++) {
	//					if (playerSelectionInformation.phonePlayerSlotTaken[j] == false) {
	//						playerSelectionInformation.phonePlayerSlot[j] = takeCharacter;
	//						playerSelectionInformation.phonePlayerSlotTaken[j] = true;
	//					}
	//				}
	//			}
				
	//			prefab = Instantiate(Resources.Load<GameObject>("Player/" + takeCharacter)) as GameObject;       //TODO: Prefab selection
	//			prefab.GetComponent<NavMeshAgent>().enabled = false;
	//			prefab.transform.position = GetPositionOfRandomPlayer(true);
	//			prefab.GetComponent<NavMeshAgent>().enabled = true;
	//			prefab.gameObject.name = "Player" + (playerCount + 1);
	//			prefab.GetComponent<BasePlayer>().PhonePlayerSlot = slot;
				
	//			OnPlayerJoined();

	//			for (int i = 0; i < playerSlotPhone.Length; i++) {
	//				if (playerSlotPhone[i] == false) {
	//					playerSlotPhone[i] = true;
	//					break;
	//				}
	//			}
	//		}
	//	}
	//}

    /// <summary>
    /// Respawns the dead players to the living players.
    /// </summary>
    public void RespawnDeadPlayers()
    {
        if (respawnPlayersAfterDeath)
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
                }
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
