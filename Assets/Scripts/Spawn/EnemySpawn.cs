using UnityEngine;
using System.Collections;

/// <summary>
/// EventHandler for each enemy spawn.
/// </summary>
public delegate void EnemySpawnedEventHandler();

/// <summary>
/// Enemy spawning script.
/// This script has to be attached to the enemy spawn.
/// </summary>
public class EnemySpawn : MonoBehaviour
{
    // Time between the spawns.
    [SerializeField]
    protected float spawnInterval;

    // The minimum distance to the player to spawn enemies.
    [SerializeField]
    protected float minDistanceToPlayer = 50f;

    // When the player is under this distance value, the spawnpoint won't spawn anything.
    [SerializeField]
    protected float deactivationDistance = 10f;

    // Current distance to the player.
    private float curentDistanceToPlayer;

    // Specifies if the spawn is allowed.
    protected bool spawnAllowed = true;

    // Interval for the distance calculation in seconds.
    [SerializeField]
    protected float calculateDistanceInterval = 0.6f;

    // Determines if the Gizmo will be drawn.
    [SerializeField]
    protected bool drawGizmo = true;

    // Event handler for the enemy spawn.
    public static event EnemySpawnedEventHandler EnemySpawned;

    /// <summary>
    /// Gets the middlepoint of the players.
    /// </summary>
    protected Vector3 PlayerMiddlePoint
    {
        get { return CameraSystem.playerBounds.center; }
    }

    void Awake()
    {
        LevelEndManager.levelExitEvent += ResetValues;
    }

    void Start()
    {
        // Start invoke call for the distance calculation.
        InvokeRepeating("CalcDistanceToPlayer", 0f, calculateDistanceInterval);
    }

    void Update()
    {
        if(!GameManager.GameManagerInstance.IsBossWave)
            SpawnEnemy();
        else
            SpawnBoss();
    }

    /// <summary>
    /// Spawns the boss.
    /// </summary>
    protected void SpawnBoss()
    {
        // Check if checkpoint is allowed to spawn.
        if (GameManager.GameManagerInstance.WaveActive && spawnAllowed && GameManager.GameManagerInstance.CurrentEnemyCount < 1)
        {
            // Check the deactivation distance and the minimum distance.
            if (curentDistanceToPlayer > deactivationDistance && curentDistanceToPlayer < minDistanceToPlayer)
            {
                // Instantiate enemy.
                GameObject boss = Instantiate(GameManager.GameManagerInstance.BossSpawnInfo.boss, 
                    PlayerMiddlePoint, GameManager.GameManagerInstance.BossSpawnInfo.boss.transform.rotation) as GameObject;

                boss.SetActive(false);
                boss.name = GameManager.GameManagerInstance.BossSpawnInfo.enemyName;

                boss.transform.position = transform.position;
                boss.SetActive(true);

                // Set increased health and attack
                BaseEnemy e = boss.GetComponent<MonoBehaviour>() as BaseEnemy;

                e.MaxHealth = GameManager.GameManagerInstance.BossSpawnInfo.ActualHealth;
                e.Health = GameManager.GameManagerInstance.BossSpawnInfo.ActualHealth;

                // Trigger event.
                OnEnemySpawn();

                //Decrease ressource pool
                GameManager.GameManagerInstance.CurrentEnemyRessourceValue -= GameManager.GameManagerInstance.BossSpawnInfo.enemyRessourceValue;

                spawnAllowed = false;
                StartCoroutine(WaitForNextSpawn());
            }
        }
    }

    /// <summary>
    /// Spawns the enemy.
    /// </summary>
    protected void SpawnEnemy()
    {
        // Check the checkpoint is allowed to spawn.
        if ( GameManager.GameManagerInstance.WaveActive && spawnAllowed && CheckEnemyCount() )
        {
            // Check the deactivation distance and the minimum distance.
            if ( curentDistanceToPlayer > deactivationDistance && curentDistanceToPlayer < minDistanceToPlayer )
            {
                // Draw active spawn points (Debug)
                Debug.DrawLine(new Vector3(transform.position.x, 0.5f, transform.position.z), CameraSystem.playerBounds.center, Color.cyan);

                // Spawn enemy.
                int index = ChooseEnemy();

                //Check Ressources and wave
                if (GameManager.GameManagerInstance.CurrentEnemyRessourceValue - GameManager.GameManagerInstance.SpawnInfo[index].enemyRessourceValue >= 0
                    && GameManager.GameManagerInstance.SpawnInfo[index].minWave <= GameManager.GameManagerInstance.Wave)
                {
                    // Instantiate enemy.
                    GameObject enemy = Instantiate(GameManager.GameManagerInstance.SpawnInfo[index].enemy, transform.position,
                        GameManager.GameManagerInstance.SpawnInfo[index].enemy.transform.rotation) as GameObject;

                    enemy.SetActive(false);
                    enemy.name = GameManager.GameManagerInstance.SpawnInfo[index].enemyName;

                    enemy.transform.position = transform.position;
                    enemy.SetActive(true);

                    // Set increased health and attack
                    BaseEnemy e = enemy.GetComponent<MonoBehaviour>() as BaseEnemy;
                    e.MeleeAttackDamage = GameManager.GameManagerInstance.SpawnInfo[index].ActualDamage;
                    e.MaxHealth = GameManager.GameManagerInstance.SpawnInfo[index].ActualHealth;
                    e.Health = GameManager.GameManagerInstance.SpawnInfo[index].ActualHealth;

                    //Decrease ressource pool
                    GameManager.GameManagerInstance.CurrentEnemyRessourceValue -= GameManager.GameManagerInstance.SpawnInfo[index].enemyRessourceValue;

                    // Trigger event.
                    OnEnemySpawn();

                    spawnAllowed = false;
                    StartCoroutine(WaitForNextSpawn());
                }
            }
        }
    }

    /// <summary>
    /// Calculates the Distance to the player.
    /// Should not be called every frame (-> Invoke repeating).
    /// </summary>
    protected void CalcDistanceToPlayer()
    {
        curentDistanceToPlayer = Vector3.Distance(transform.position, CameraSystem.playerBounds.center);
    }

    /// <summary>
    /// Waits for the next spawn.
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForNextSpawn()
    {
        yield return new WaitForSeconds(spawnInterval);
        spawnAllowed = true;
    }

    /// <summary>
    /// Checks if the enemycount is under the max count.
    /// </summary>
    /// <returns>true: Spawn allowed, false: Spawn not allowed</returns>
    protected bool CheckEnemyCount()
    {
        if (GameManager.GameManagerInstance.CurrentEnemyCount < GameManager.GameManagerInstance.MaxEnemyActiveCount)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Chooses an enemy based on it's probability and returns the index of the spawn info array.
    /// </summary>
    protected int ChooseEnemy()
    { 
        // Total probability.
        float total = 0;

        //Random value between 0 and the total probability.
        float randomValue = 0;

        // Calc total probability
        foreach (GameManager.SpawnInformation prob in GameManager.GameManagerInstance.SpawnInfo)
            total += prob.spawnProbability;

        randomValue = Random.value * total;

        for (int i = 0; i < GameManager.GameManagerInstance.SpawnInfo.Length; i++)
        {
            if (randomValue < GameManager.GameManagerInstance.SpawnInfo[i].spawnProbability)
                return i;
            else
                randomValue -= GameManager.GameManagerInstance.SpawnInfo[i].spawnProbability;
        }

        return GameManager.GameManagerInstance.SpawnInfo.Length - 1;
    }

    /// <summary>
    /// Event method for the enemy spawn.
    /// </summary>
    protected void OnEnemySpawn()
    {
        if (EnemySpawned != null)
            EnemySpawned();
    }

    /// <summary>
    /// Draws the min activation distance and the deactivation distance.
    /// </summary>
    protected void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, deactivationDistance);
        }
    }

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected virtual void ResetValues()
    {
        EnemySpawned = null;
    }
}
