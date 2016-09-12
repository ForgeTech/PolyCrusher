using UnityEngine;

/// <summary>
/// Event handler for a killed boss.
/// </summary>
/// <param name="boss">Boss who died.</param>
public delegate void BossKilledEventHandler(BossEnemy boss);

/// <summary>
/// Event handler for a spawned boss.
/// </summary>
/// <param name="boss"></param>
public delegate void BossSpawnedEventHandler(BossEnemy boss);

public class BossEnemy : BaseEnemy
{
    #region Inner Classes
    /// <summary>
    /// Information class for the AI Phases.
    /// </summary>
    [System.Serializable]
    public class PhaseInformation
    {
        /// <summary>
        /// Specifies if the phase is enabled.
        /// </summary>
        public bool phaseEnabled;

        /// <summary>
        /// The probability of the phase.
        /// </summary>
        [Range(0f, 1f)]
        public float phaseProbability;

        [Tooltip("The time after the phase should be exited. (in seconds)")]
        [Range(0f, 30f)]
        public float phaseTime = 5f;
    }

    /// <summary>
    /// Information class for the mob spawn phase.
    /// </summary>
    [System.Serializable]
    public class MobSpawnPhaseInformation
    {
        /// <summary>
        /// Specifies if the phase is enabled.
        /// </summary>
        public bool phaseEnabled;

        /// <summary>
        /// Spawnradius of the mobs. They spawn around the boss.
        /// </summary>
        public float spawnRadius = 8f;

        /// <summary>
        /// Spawn interval of the mobs in seconds.
        /// </summary>
        public float spawnInterval = 0.3f;

        /// <summary>
        /// Duration of the phase.
        /// </summary>
        [Tooltip("The time after the phase should be exited. (in seconds)")]
        [Range(0f, 30f)]
        public float phaseTime = 5f;

        /// <summary>
        /// Reference to the mob prefab.
        /// </summary>
        public GameObject mobPrefab;

        /// <summary>
        /// Spawns when the percentage of the health is reached.
        /// </summary>
        [Tooltip("Spawns when the percentage of the health is reached.")]
        [Range(0f, 1f)]
        public float[] spawnPercentage;
    }

    [System.Serializable]
    public class SprintPhaseInformation
    {
        /// <summary>
        /// Specifies if the phase is enabled or not.
        /// </summary>
        public bool enabled;

        /// <summary>
        /// The speed of the sprint attack.
        /// </summary>
        [Range(10f, 30f)]
        public float sprintSpeed = 17f;

        /// <summary>
        /// After damage value has been reached, the boss will sprint to a player.
        /// </summary>
        public int sprintTriggerDamage = 100;

        /// <summary>
        /// The current damage value.
        /// </summary>
        //[HideInInspector]
        public int currentDamage = 0;
    }

    [System.Serializable]
    public class PlayerLifeSettings
    {
        [Range(0, 1f)]
        [Tooltip("When there is one player, the boss should have X percent of his normal health.")]
        public float onePlayer = 0.5f;

        [Range(0, 1f)]
        [Tooltip("When there are two players, the boss should have X percent of his normal health.")]
        public float twoPlayers = 0.65f;

        [Range(0, 1f)]
        [Tooltip("When there are three players, the boss should have X percent of his normal health.")]
        public float threePlayers = 0.85f;
    }
    #endregion

    #region Member variables
    //[Header("Boss player count life settings")]

    [Header("Boss life settings based on playercount")]
    [SerializeField]
    PlayerLifeSettings lifeSetting;

    [SerializeField]
    [Tooltip("Additional health value which will be added per boss wave.")]
    protected int additionHealthPerWave = 1000;

    [SerializeField]
    [Header("Range attack values")]
    [Tooltip("The damage of the ranged attack.")]
    protected int rangedAttackDamage = 10;

    [SerializeField]
    [Tooltip("The interval of the ranged attack")]
    protected float rangedAttackInterval = 0.5f;

    [SerializeField]
    [Tooltip("Range of the ranged attack")]
    protected float rangedAttackRange = 10f;

    [Space(5)]
    [Tooltip("Phase 1: Melee, Phase 2: Melee and Ranged, Phase 3: Melee, Ranged, Special Ability with mob spawn.")]
    [Header("Boss ability settings")]

    [SerializeField]
    protected PhaseInformation meleePhase;

    [Space(4)]
    [SerializeField]
    [Tooltip("Ranged attack phase")]
    protected PhaseInformation rangedPhase;

    [Space(4)]
    [SerializeField]
    [Tooltip("Special ability phase")]
    protected PhaseInformation specialPhase;

    [Space(5)]
    [Header("Sprint settings")]
    [SerializeField]
    [Tooltip("Settings for the sprint phase.")]
    protected SprintPhaseInformation sprintPhase;

    [Space(5)]
    [Header("Mob Spawn settings")]
    [SerializeField]
    MobSpawnPhaseInformation mobSpawnPhase;

    [Space(4)]
    [Header("Idle settings")]
    [Tooltip("Time which the boss stays in idle after he chooses another state.")]
    [SerializeField]
    protected float idleTime = 2f;

    [Space(5)]
    [Header("Attack visualization and settings")]
    [SerializeField]
    protected GameObject meleeAreaOfDamage;

    [Tooltip("Time of the area of damage.")]
    [SerializeField]
    protected float areaOfDamageTime = 1.5f;

    [SerializeField]
    [Tooltip("The damage radius of the attack.")]
    protected float areoOfDamageRadius = 5f;

    [Space(4)]
    [SerializeField]
    [Tooltip("The prefab reference to the boss bullet.")]
    protected GameObject rangedBullet;

    [SerializeField]
    [Tooltip("Number of bullets of the ranged")]
    protected int numberOfBullets = 8;
    
    [SerializeField]
    [Tooltip("Angle of the bullet spread.")]
    protected float spreadAngle = 70f;

    [Space(4)]
    [SerializeField]
    [Tooltip("Prefab of the meteor attack.")]
    protected GameObject meteorAttackPrefab;

    [SerializeField]
    [Tooltip("The spawn height of the meteorits.")]
    protected float meteorSpawnHeight = 10f;

    // Gravestone model
    [Space(4)]
    [SerializeField]
    [Tooltip("Gravestone")]
    protected GameObject gravestone;

    [SerializeField]
    [Tooltip("Death Particle System")]
    protected GameObject deathParticles;

    [SerializeField]
    [Tooltip("Death sound")]
    protected AudioClip deathSound;

    [Space(5)]
    [SerializeField]
    [Header("Boss UI")]
    [Tooltip("Health bar")]
    protected UnityEngine.UI.Image healthLevel;

    // Turbine particles.
    protected ParticleSystem[] turbineParticles;

    // The overall spawn count of the boss.
    private static int spawnCount = 0;
    #endregion

    #region Properties
    /// <summary>
    /// Gets melee phase.
    /// </summary>
    public PhaseInformation MeleePhase
    {
        get { return this.meleePhase; }
    }

    /// <summary>
    /// Gets the ranged phase.
    /// </summary>
    public PhaseInformation RangedPhase
    {
        get { return this.rangedPhase; }
    }

    /// <summary>
    /// Gets the special phase
    /// </summary>
    public PhaseInformation SpecialPhase
    {
        get { return this.specialPhase; }
    }

    /// <summary>
    /// Gets sprint phase information.
    /// </summary>
    public SprintPhaseInformation SprintPhase
    {
        get { return this.sprintPhase; }
    }

    /// <summary>
    /// Gets mob spawn phase information.
    /// </summary>
    public MobSpawnPhaseInformation MobSpawnPhase
    {
        get { return this.mobSpawnPhase; }
    }

    /// <summary>
    /// Gets the idle time.
    /// </summary>
    public float IdleTime
    {
        get { return this.idleTime; }
    }

    /// <summary>
    /// Gets the reference of the area of damage prefab.
    /// </summary>
    public GameObject MeleeAreaOfDamage
    {
        get { return this.meleeAreaOfDamage; }
    }

    /// <summary>
    /// Gets the area of damage time.
    /// </summary>
    public float AreaOfDamageTime
    {
        get { return this.areaOfDamageTime; }
    }

    /// <summary>
    /// Gets the area of damage radius.
    /// </summary>
    public float AreoOfDamageRadius
    {
        get { return this.areoOfDamageRadius; }
    }

    /// <summary>
    /// Gets the ranged attack range.
    /// </summary>
    public float RangedAttackRange
    {
        get { return this.rangedAttackDamage; }
    }

    /// <summary>
    /// Gets the ranged attack interval.
    /// </summary>
    public float RangedAttackInterval
    {
        get { return this.rangedAttackInterval; }
    }

    /// <summary>
    /// Gets the ranged bullet.
    /// </summary>
    public GameObject RangedBullet
    {
        get { return this.rangedBullet; }
    }

    /// <summary>
    /// Gets the number of bullets.
    /// </summary>
    public int NumberOfBullets
    {
        get { return this.numberOfBullets; }
    }

    /// <summary>
    /// Gets the spread angle of the bullets.
    /// </summary>
    public float SpreadAngle
    {
        get { return this.spreadAngle; }
    }

    /// <summary>
    /// Gets the meteor attack prefab.
    /// </summary>
    public GameObject MeteorAttackPrefab
    {
        get { return this.meteorAttackPrefab; }
    }

    /// <summary>
    /// Gets the meteor spawn height.
    /// </summary>
    public float MeteorSpawnHeight
    {
        get { return this.meteorSpawnHeight; }
    }

    /// <summary>
    /// Gets the particle systems.
    /// </summary>
    public ParticleSystem[] TurbineParticles
    {
        get { return this.turbineParticles; }
    }
    #endregion

    // Event for a killed boss.
    public static event BossKilledEventHandler BossKilled;

    // Event for a spawned boss.
    public static event BossSpawnedEventHandler BossSpawned;

    protected override void Start()
    {
        base.Start();
        spawnCount++;

        // Set boss health based on the playercount
        if (PlayerManager.PlayerCount == 1)
        {
            MaxHealth = (int)(lifeSetting.onePlayer * Health);
            Health = MaxHealth;

            BossShield s = GetComponentInChildren<BossShield>();
            if (s != null)
                s.gameObject.SetActive(false);
        }
        else if (PlayerManager.PlayerCount == 2)
        {
            MaxHealth = (int)(lifeSetting.twoPlayers * Health);
            Health = MaxHealth;
        }
        else if (PlayerManager.PlayerCount == 3)
        {
            MaxHealth = (int)(lifeSetting.threePlayers * Health);
            Health = MaxHealth;
        }

        // Add health value when the boss is spawned more often.
        if (spawnCount > 1)
        {
            MaxHealth = MaxHealth + ((spawnCount - 1) * additionHealthPerWave);
            Health = MaxHealth;
        }

        turbineParticles = GetComponentsInChildren<ParticleSystem>();

        // Fire spawn event.
        OnBossSpawned();
    }

    /// <summary>
    /// Initializes the FSM.
    /// </summary>
    protected override void MakeFSM()
    {
        // Set up FSM.
        BossIdle idle = new BossIdle(this);
        idle.AddTransition(Transition.DecisionMelee, StateID.BossAttackMelee);
        idle.AddTransition(Transition.DecisionRanged, StateID.BossAttackRanged);
        idle.AddTransition(Transition.DecisionSpecial, StateID.BossAttackSpecial);
        idle.AddTransition(Transition.DecisionMobSpawn, StateID.BossMobSpawn);
        idle.AddTransition(Transition.DecisionSprint, StateID.BossSprint);

        BossAttackMelee attackMelee = new BossAttackMelee(MeleePhase.phaseTime, StateID.BossAttackMelee);
        attackMelee.AddTransition(Transition.AttackFinished, StateID.BossIdle);
        attackMelee.AddTransition(Transition.LostPlayerAttackRange, StateID.BossWalk);
        
        BossWalk attackPhaseWalk = new BossWalk(AttackRange, StateID.BossWalk);
        attackPhaseWalk.AddTransition(Transition.ReachedDestination, StateID.BossAttackMelee);


        BossAttackRanged attackRanged = new BossAttackRanged(RangedPhase.phaseTime, StateID.BossAttackRanged);
        attackRanged.AddTransition(Transition.AttackFinished, StateID.BossIdle);
        attackRanged.AddTransition(Transition.LostPlayerAttackRange, StateID.BossWalkRanged);

        BossWalk attackRangedWalk = new BossWalk(RangedAttackRange, StateID.BossWalkRanged);
        attackRangedWalk.AddTransition(Transition.ReachedDestination, StateID.BossAttackRanged);


        BossAttackSpecial attackSpecial = new BossAttackSpecial(SpecialPhase.phaseTime, StateID.BossAttackSpecial);
        attackSpecial.AddTransition(Transition.AttackFinished, StateID.BossIdle);
        attackSpecial.AddTransition(Transition.LostPlayerAttackRange, StateID.BossWalkSpecial);

        BossWalk walkSpecial = new BossWalk(RangedAttackRange, StateID.BossWalkSpecial);
        walkSpecial.AddTransition(Transition.ReachedDestination, StateID.BossAttackSpecial);

        BossMobSpawn mobSpawn = new BossMobSpawn(MobSpawnPhase.phaseTime, StateID.BossMobSpawn);
        mobSpawn.AddTransition(Transition.AttackFinished, StateID.BossIdle);

        BossSprint bossSprint = new BossSprint(StateID.BossSprint, this);
        bossSprint.AddTransition(Transition.ReachedDestination, StateID.BossIdle);

        fsm = new FSMSystem();
        fsm.AddState(idle);
        fsm.AddState(attackMelee);
        fsm.AddState(attackPhaseWalk);
        fsm.AddState(attackRanged);
        fsm.AddState(attackRangedWalk);
        fsm.AddState(attackSpecial);
        fsm.AddState(walkSpecial);
        fsm.AddState(mobSpawn);
        fsm.AddState(bossSprint);
    }

    protected override void Update()
    {
        base.Update();

        // HUD Elements
        UpdateHUDElements();
    }

    public override void EnemySpawnScaleTween()
    {
        // Spawn scale
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        // Only tween the boss with the old tweening system, because with LeanTween there are some problems with the health bar.
        StartCoroutine(transform.ScaleTo(originalScale, 0.7f, AnimCurveContainer.AnimCurve.pingPong.Evaluate));
    }

    protected override void DestroyEnemy()
    {
        //Disable
        targetPlayer = null;
        GetComponent<NavMeshAgent>().Stop();
        GetComponent<NavMeshAgent>().updateRotation = false;
        GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        //Create Gravestone
        Instantiate(gravestone, transform.position, gravestone.transform.rotation);

        //Create Particles
        SoundManager.SoundManagerInstance.Play(deathSound, Vector3.zero, 0.8f, 1f, AudioGroup.Effects);
        Destroy(Instantiate(deathParticles, transform.position, gravestone.transform.rotation), 10);

        //Animation
        if (anim != null)
            anim.SetBool("Death", true);

        //Event.
        OnBossKilled();

        //Scale Fade out
        StartCoroutine(transform.ScaleFrom(Vector3.zero, lifeTimeAfterDeath, AnimCurveContainer.AnimCurve.downscale.Evaluate));

        //Destroy
        Destroy(this.gameObject, lifeTimeAfterDeath);
    }

    /// <summary>
    /// Event method for the Boss kill event.
    /// </summary>
    protected virtual void OnBossKilled()
    {
        if (BossKilled != null)
            BossKilled(this);

    }

    /// <summary>
    /// Event method for the Boss spawn event.
    /// </summary>
    protected virtual void OnBossSpawned()
    {
        if (BossSpawned != null)
            BossSpawned(this);
    }

    /// <summary>
    /// Draws the object some damage and lowers the health.
    /// </summary>
    /// <param name="damage">The damage dealt.</param>
    /// /// <param name="damageDealer">The damage dealer.</param>
    public override void TakeDamage(int damage, MonoBehaviour damageDealer)
    {
        base.TakeDamage(damage, damageDealer);

        sprintPhase.currentDamage += damage;        
    }

    /// <summary>
    /// Updates the values of the HUD elements.
    /// </summary>
    private void UpdateHUDElements()
    {
        // Update Health
        if (healthLevel != null)
        {
            healthLevel.fillAmount = (float)Health / (float)maxHealth;
        }
    }

    /// <summary>
    /// Resets the boss event handler.
    /// </summary> 
    protected override void ResetValues()
    {
        base.ResetValues();
        BossKilled = null;
        BossSpawned = null;
        spawnCount = 0;
    }
}
