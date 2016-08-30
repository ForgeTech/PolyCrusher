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

    //Rumble manager for accessing rumble functions
    private RumbleManager rumbleManager;

    // The time of a whole game.
    private static TimeUtil playTime = null;

    // Start time for time measurement.
    private float startTime = 0;

    private float overallTime = 0f;

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


        rumbleManager = RumbleManager.Instance;
        if (rumbleManager == null)
        {
            Debug.LogError("Rumblemanager is null");
        }

        GameObject g = GameObject.FindGameObjectWithTag("GlobalScripts");
        if (g != null)
        {
            playerSelectionInformation = g.GetComponent<PlayerSelectionContainer>();
            if (playerSelectionInformation != null)
            {
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
                            prefab.GetComponent<BasePlayer>().RumbleManager = rumbleManager;
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
            else
            {
                Debug.LogError("No Player Selection Container found!");
            }
        }
        else
        {
            Debug.Log("No Global Scripts GameObject found");
        }

        playTime = new TimeUtil();
        startTime = 0f;
        measureTime = true;

        // start session
        DataCollector.instance.startSession();
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
            overallTime += Time.deltaTime;
            float endTime = overallTime;
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
