using UnityEngine;
using System.Collections;
using System.Text;
using System;

/// <summary>
/// Eventhandler for the wave start.
/// </summary>
public delegate void WaveStartedEventHandler();

/// <summary>
/// Eventhandler for the wave end.
/// </summary>
public delegate void WaveEndedEventHandler();

/// <summary>
/// Enumeration for the different game modes.
/// </summary>
public enum GameMode
{
    NormalMode = 0,
    YOLOMode = 1
}

/// <summary>
/// Implements the wave system and the spawning decision for the enemies.
/// The game manager is implemented with the Singleton-Pattern.
/// </summary>
[System.Serializable]
public class GameManager : MonoBehaviour
{
    #region Inner classes
    /// <summary>
    /// Spawn information for the enemies.
    // </summary>
    [System.Serializable]
    public class SpawnInformation
    {
        // Enemy to spawn.
        public GameObject enemy;

        // Probability to spawn. (Range: 0 - 1);
        public float spawnProbability;

        // Ressource value for the enemy.
        public int enemyRessourceValue = 1;

        // Minimum wave to spawn.
        public float minWave;

        // The name of the enemy.
        public string enemyName = "Enemy";

        // The actual health. This value will be used to increase the health per wave.
        protected int actualHealth;
        [HideInInspector]
        public float preciseHealth;

        // The actual health. This value will be used to increase the health per wave.
        protected int actualDamage;
        [HideInInspector]
        public float preciseDamage;


        /// <summary>
        /// Gets or sets the actual health.
        /// </summary>
        public int ActualHealth
        {
            get { return this.actualHealth; }
            set { this.actualHealth = value; }
        }

        /// <summary>
        /// Gets or sets the actual damage.
        /// </summary>
        public int ActualDamage
        {
            get { return this.actualDamage; }
            set { this.actualDamage = value; }
        }
    }

    /// <summary>
    /// Spawn information for the boss enemies.
    /// </summary>
    [System.Serializable]
    public class BossSpawnInformation
    {
        /// <summary>
        /// Reference to the boss.
        /// </summary>
        public GameObject boss;

        // Ressource value for the enemy.
        public int enemyRessourceValue = 1;

        // The name of the boss.
        public string enemyName = "BossEnemy";

        // Every x wave, the boss enemy will be spawned.
        [Tooltip("Every x wave the boss enemy will be spawned.")]
        public int spawnEveryXWave = 5;

        // Variation of the boss spawn.
        public int waveSpawnVariation = 3;

        // The actual health. This value will be used to increase the health per wave.
        protected int actualHealth;
        [HideInInspector]
        public float preciseHealth;


        /// <summary>
        /// Gets or sets the actual health.
        /// </summary>
        public int ActualHealth
        {
            get { return this.actualHealth; }
            set { this.actualHealth = value; }
        }
    }
    #endregion

    #region Class Members

    // Current game mode
    [SerializeField]
    protected GameMode gameMode = GameMode.NormalMode;

    // Spawn information.
    [SerializeField]
    protected SpawnInformation[] spawnInfo;

    // Boss spawn information.
    [SerializeField]
    protected BossSpawnInformation bossSpawnInfo;

    [SerializeField]
    protected bool specialWaveModeEnabled = true;

    // Determines if special waves are currently enabled.
    private bool isCurrentlySpecialWave = false;

    // The probability of the ocurrence of special waves [0, 1].
    [SerializeField]
    [Range(0, 1)]
    protected float specialWaveProbablity = 0.05f;

    // Represenation of the game manager
    public static GameManager gameManagerInstance;

    // The spawn points of the enemies.
    protected GameObject[] enemySpawnPoints;

    // Event handler for the wave start.
    public static event WaveStartedEventHandler WaveStarted;

    // Event handler for the wave end.
    public static event WaveEndedEventHandler WaveEnded;

    // The wave count.
    protected int wave = 0;

    //[Header("==========Ressource Settings============")]
    // The max enemy ressources
    [SerializeField]
    protected int enemyRessourcePool = 20;
    protected double preciseRessourcePool = 20.0;

    // Current ressource pool of the enemies.
    [SerializeField]
    protected int currentEnemyRessourceValue;

    // The accumulated ressource value.
    protected int accumulatedRessourceValue;

    //[Header("==========Enemy Count Settings==========")]
    // Describes how many enemies are allowed to be active at the same time.
    [SerializeField]
    protected int maxEnemyActiveCount = 10;

    // The current enemy count.
    [SerializeField]
    protected int currentEnemyCount = 0;

    // Time between the waves.
    [SerializeField]
    protected float timeBetweenWave = 5f;

    // Determines if the wave is active or if there is a pause.
    protected bool waveActive = false;

    // Determines if the wav is a boss wave or not.
    protected bool isBossWave = false;

    // Every x wave the boss will be spawned.
    protected int bossSpawnWaves = 0;

    #region Increase per wave variables

    //[Header("=========Wave Increase Settings=========")]
    [Tooltip("The increase factor of the enemy ressources for every wave. Value should be between 0 and 1!")]
    [SerializeField]
    protected double enemyRessourceIncreaseFactor = 0.19;

    [Tooltip("Multiplier for the enemy ressource increase Factor. This value increases the ressource factor each wave.")]
    [SerializeField]
    protected double ressourceIncreaseFactorMultiplier = 1.01;

    [Space(10f)]

    [Tooltip("The increase factor of the enemy count for every wave. Value should be between 0 and 1!")]
    [SerializeField]
    protected float enemyCountIncreaseFactor = 0.2f;

    [Space(10f)]

    [Tooltip("The decrease factor of the time between the waves. Value should be between 0 and 1!")]
    [SerializeField]
    protected float timeBetweeenWaveDecreaseFactor = 0.05f;

    [Space(10f)]

    [Tooltip("The increase factor of the enemy health for every wave. Value should be between 0 and 1!")]
    [SerializeField]
    protected float enemyHealthIncreaseFactor = 0.1f;

    [Tooltip("The increase factor of the enemy damage for every wave. Value should be between 0 and 1!")]
    [SerializeField]
    protected float enemyDamageIncreaseFactor = 0.1f;

    [SerializeField]
    protected float[] playerCountHealthMultiplier;
    #endregion

    #endregion

    #region Properties
    /// <summary>
    /// Public reference of the game manager.
    /// </summary>
    public static GameManager GameManagerInstance
    {
        get
        {
            //If the instance isn't set yet, it will be set (Happens only the first time!)
            if (gameManagerInstance == null)
                gameManagerInstance = GameObject.FindObjectOfType<GameManager>();

            return gameManagerInstance;
        }
    }

    public GameMode CurrentGameMode { get { return gameMode; } }
    public int Wave { get { return wave; } }
    public SpawnInformation[] SpawnInfo { get { return spawnInfo; } }
    public BossSpawnInformation BossSpawnInfo { get { return bossSpawnInfo; } }
    public int AccumulatedRessourceValue { get { return accumulatedRessourceValue; } }
    public bool SpecialWaveModeEnabled { get { return specialWaveModeEnabled; } }
    public bool IsCurrentlySpecialWave { get { return isCurrentlySpecialWave; } }
    public float SpecialWaveProbablity { get { return specialWaveProbablity; } }

    public int EnemyRessourcePool
    {
        get { return enemyRessourcePool; }
        set { enemyRessourcePool = value; }
    }

    public int MaxEnemyActiveCount
    {
        get { return maxEnemyActiveCount; }
        set { maxEnemyActiveCount = value; }
    }
    
    public bool WaveActive
    {
        get { return waveActive; }
        protected set { waveActive = value; }
    }

    public bool IsBossWave
    {
        get { return isBossWave; }
        protected set { isBossWave = value; }
    }

    /// <summary>
    /// Gets or sets the current enemy ressource pool.
    /// </summary>
    public int CurrentEnemyRessourceValue
    {
        get { return currentEnemyRessourceValue; }
        set
        {
            if (value >= 0)
                currentEnemyRessourceValue = value;
            else
                CurrentEnemyRessourceValue = 0;

            if (currentEnemyRessourceValue == 0 && currentEnemyCount == 0)
            {
                Debug.Log("GameManager: No ressources and 0 enemies left -> Wave ended.");
                EndWave();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current enemy count.
    /// </summary>
    public int CurrentEnemyCount
    {
        get { return currentEnemyCount; }
        protected set 
        {
            currentEnemyCount = value;

            if (currentEnemyRessourceValue == 0 && currentEnemyCount == 0)
            {
                Debug.Log("GameManager: No ressources and 0 enemies left -> Wave ended.");
                EndWave();
            }
        }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;

        // Increase one time before actual wave starts
        enemyRessourceIncreaseFactor *= ressourceIncreaseFactorMultiplier;
        preciseRessourcePool = EnemyRessourcePool;
    }

    // Use this for initialization
	private void Start () 
    {
        // Search for enemy spawn points.
        enemySpawnPoints = GameObject.FindGameObjectsWithTag("EnemySpawn");

        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            enemySpawnPoints = null;

        // Register event methods.
        EnemySpawn.EnemySpawned += EnemySpawned;
        BaseEnemy.EnemyKilled += EnemyDied;
        BossEnemy.BossKilled += BossDied;

        //Proces Enemy information.
        ProcessEnemyData();

        //Trigger first waiting sequence. This will start the first wave.
        StartCoroutine(WaitForNextWave());

        // Init variables
        accumulatedRessourceValue = 0;
	}

    /// <summary>
    /// Starts a new wave.
    /// </summary>
    protected void StartNextWave()
    {
        this.wave++;

        if (wave == 1)
        {
            //fire game start event
            if(gameMode == GameMode.NormalMode)
                new Event(Event.TYPE.gameStart).addPlayerCount().addPlayerCharacters().addMobilePlayers(0).addLevel().addMode("normal").send();
            else if(gameMode == GameMode.YOLOMode)
                new Event(Event.TYPE.gameStart).addPlayerCount().addPlayerCharacters().addMobilePlayers(0).addLevel().addMode("yolo").send();
        }

        // Calculate the boss wave spawns at the start of the game.
        if (wave <= 1)
        {
            bossSpawnWaves = UnityEngine.Random.Range(BossSpawnInfo.spawnEveryXWave, BossSpawnInfo.spawnEveryXWave + BossSpawnInfo.waveSpawnVariation + 1);
        }

        // Calculate boss wave
        if (this.wave % bossSpawnWaves == 0)
            isBossWave = true;

        // If the wave is beyond 1, increase the settings to make the game harder.
        if (Wave > 1)
            CalculateNextWaveValues();

        //Debug
        StringBuilder debugMessage = new StringBuilder("[GameManager]: <b>Wave ");
        debugMessage.Append(wave).Append(" start!</b> [ ").Append(System.DateTime.Now.ToString())
            .Append("]").Append(" Boss: ").Append(IsBossWave).Append(", Special: ").Append(IsCurrentlySpecialWave);
        Debug.Log(debugMessage.ToString());

        // Check if it is a boss wave.
        if (isBossWave)
            bossSpawnInfo.enemyRessourceValue = EnemyRessourcePool;

        CurrentEnemyRessourceValue = EnemyRessourcePool;
        WaveActive = true;

        OnWaveStarted();
    }

    /// <summary>
    /// Increases specific values to increase the game difficulty.
    /// </summary>
    protected void CalculateNextWaveValues()
    {
        // Increase enemy ressource increase factor
        enemyRessourceIncreaseFactor *= ressourceIncreaseFactorMultiplier;

        // Increase the enemy ressources.
        preciseRessourcePool += preciseRessourcePool * enemyRessourceIncreaseFactor;
        EnemyRessourcePool = (int) Math.Round(preciseRessourcePool);

        // Increase the enemy count.
        MaxEnemyActiveCount += (int) (MaxEnemyActiveCount * enemyCountIncreaseFactor);

        // Decrease time between wave.
        timeBetweenWave *= (1f - timeBetweeenWaveDecreaseFactor);

        // Increase enemy damage and health.
        for (int i = 0; i < spawnInfo.Length; i++)
        {
            spawnInfo[i].preciseHealth += spawnInfo[i].preciseHealth * enemyHealthIncreaseFactor;
            spawnInfo[i].ActualHealth = (int) spawnInfo[i].preciseHealth;

            spawnInfo[i].preciseDamage += spawnInfo[i].preciseDamage * enemyDamageIncreaseFactor;
            spawnInfo[i].ActualDamage = (int) spawnInfo[i].preciseDamage;
        }

        // Increase boss and health.
        bossSpawnInfo.preciseHealth += bossSpawnInfo.ActualHealth * enemyHealthIncreaseFactor;
        bossSpawnInfo.ActualHealth = (int) bossSpawnInfo.preciseHealth;

        // Set the accumulated ressource value back to 0.
        accumulatedRessourceValue = 0;

        // Calculates if the wave is a special wave or not.
        if (SpecialWaveModeEnabled && !IsBossWave) {
            isCurrentlySpecialWave = UnityEngine.Random.Range(0.0f, 1.0f) < SpecialWaveProbablity;
        }
    }

    /// <summary>
    /// Modifies the enemy health based on the player count.
    /// </summary>
    private void ModifyEnemyHealthBasedOnPlayerCount(SpawnInformation spawnInfo)
    {
        if (PlayerManager.PlayerCountInGameSession > 0)
        {
            spawnInfo.preciseHealth = spawnInfo.preciseHealth * playerCountHealthMultiplier[PlayerManager.PlayerCountInGameSession - 1];
            spawnInfo.ActualHealth = (int) spawnInfo.preciseHealth;
        }
    }

    /// <summary>
    /// Processes the enemy data and saves the initial health and damage values into the spawn information.
    /// </summary>
    protected void ProcessEnemyData()
    {
        for (int i = 0; i < spawnInfo.Length; i++)
        {
            // Health
            spawnInfo[i].ActualHealth = spawnInfo[i].enemy.GetComponent<BaseEnemy>().Health;
            spawnInfo[i].preciseHealth = spawnInfo[i].ActualHealth;

            ModifyEnemyHealthBasedOnPlayerCount(spawnInfo[i]);

            // Damage
            spawnInfo[i].ActualDamage = spawnInfo[i].enemy.GetComponent<BaseEnemy>().MeleeAttackDamage;
            spawnInfo[i].preciseDamage = spawnInfo[i].ActualDamage;
        }
        BossSpawnInfo.ActualHealth = BossSpawnInfo.boss.GetComponent<BaseEnemy>().MaxHealth;
    }

    /// <summary>
    /// Ends a wave.
    /// </summary>
    protected void EndWave()
    {
        WaveActive = false;
        isBossWave = false;
        StartCoroutine(WaitForNextWave());
        CheckSteamAchievement();

        OnWaveEnded();
    }

    private void CheckSteamAchievement()
    {
        // Last man standing
        if (PlayerManager.PlayerCountInGameSession == 4 && PlayerManager.PlayerCount == 1)
        {
            BasePlayer player = GameObject.FindGameObjectWithTag("Player").GetComponent<BasePlayer>();
            if(player != null && (player.Health <= player.MaxHealth * 0.1f))
                BaseSteamManager.Instance.LogAchievementData(AchievementID.ACH_LAST_MAN_STANDING);
        }
    }

    /// <summary>
    /// Event method for the wave start.
    /// </summary>
    protected virtual void OnWaveStarted()
    {
        if (WaveStarted != null)
            WaveStarted();
    }

    /// <summary>
    /// Event method for the wave end.
    /// </summary>
    protected virtual void OnWaveEnded()
    {
        if (WaveEnded != null)
            WaveEnded();
    }

    /// <summary>
    /// Increases the current enemy count.
    /// Event handler method.
    /// </summary>
    protected void EnemySpawned()
    {
        this.CurrentEnemyCount++;
    }

    /// <summary>
    /// Decreases the enemy count.
    /// </summary>
    protected void EnemyDied(BaseEnemy enemy)
    {
        this.CurrentEnemyCount--;
        // Add the ressource value
        for (int i = 0; i < spawnInfo.Length; i++)
        {
            MonoBehaviour m = spawnInfo[i].enemy.GetComponent<MonoBehaviour>();

            if (m != null && m is BaseEnemy)
            {
                BaseEnemy e = m as BaseEnemy;

                // Add the ressource value if the name is equal.
                if (e.EnemyName == enemy.EnemyName)
                    this.accumulatedRessourceValue += spawnInfo[i].enemyRessourceValue;
            }
        }
    }

    /// <summary>
    /// Decreases the enemy count.
    /// </summary>
    /// <param name="e"></param>
    protected void BossDied(BossEnemy e)
    {
        this.CurrentEnemyCount--;
        
        MonoBehaviour m = bossSpawnInfo.boss.GetComponent<MonoBehaviour>();

        if (m != null && m is BossEnemy && e != null)
        {
            BossEnemy b = m as BossEnemy;

            // Add the ressource value if the name is equal.
            if (b.EnemyName == e.EnemyName)
            {
                this.accumulatedRessourceValue += BossSpawnInfo.enemyRessourceValue;
            }
        }
        else if (e == null)
        {
            // Add the ressource also if the incoming boss is null -> the boss may be destroyed in a bad constellation
            this.accumulatedRessourceValue += bossSpawnInfo.enemyRessourceValue;
        }
    }

    /// <summary>
    /// Increases the current enemy count when the boss spawns.
    /// </summary>
    protected void BossSpawned()
    {
        this.CurrentEnemyCount++;
    }

    protected IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWave);
        StartNextWave();
    }

    /// <summary>
    /// Kills all the active enemies, based on the "Enemy" tag.
    /// </summary>
    public void KillAllEnemies()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Enemy");

        for(int i = 0; i < obj.Length; i++)
        {
            BaseEnemy enemy = obj[i].GetComponent<BaseEnemy>();

            if (enemy != null)
            {
                enemy.InstantKill(this);
            }
        }
    }

    protected void ResetValues()
    {
        WaveStarted = null;
        WaveEnded = null;
    }
    #endregion
}
