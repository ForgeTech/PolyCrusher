using UnityEngine;
using System.Collections;

/// <summary>
/// Event handler for an enemy death.
/// </summary>
public delegate void EnemyKilledEventHandler(BaseEnemy enemy);

/// <summary>
/// Base enemy class with basic enemy behaviour.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IDamageable, IAttackable
{
    #region Class members

    [Header("Health values")]
    // Actual health of the enemy.
    [SerializeField]
    protected int health = 100;

    // Min health of the enemy.
    [SerializeField]
    protected int minHealth = 0;

    // Max health of the enemy.
    [SerializeField]
    protected int maxHealth = 100;

    // Specifies if the enemy can shoot.
    protected bool canShoot;

    [Space(5)]
    [Header("Enemy name")]
    // The enemy name.
    [SerializeField]
    protected string enemyName = "Enemy";

    private Material enemyMaterial;

    /// <summary>
    /// Returns enemy name.
    /// </summary>
    public string EnemyName
    {
        get { return this.enemyName; }
    }

    [Space(5)]
    [Header("Movement")]
    // Speed of the enemy.
    [Tooltip("The movement speed overwrites the NavAgent speed!")]
    [SerializeField]
    protected float movementSpeed = 5f;

    // The initial movementSpeed (Is used, because the normal movementSpeed can be changed.)
    private float initialMovementSpeed;

    [SerializeField]
    [Tooltip("The base movement speed multiplier of the walk animation.")]
    protected float movementAnimationSpeed = 1f;

    [Space(5)]
    [Header("Attack values")]
    // The damage of the melee attack.
    [SerializeField]
    protected int meleeAttackDamage = 5;

    // The push away force of attack.
    [SerializeField]
    protected float pushAwayForce = 2.5f;

    // The attack interval in seconds
    [SerializeField]
    protected float attackInterval = 1.2f;

    // The effective range of the attack
    [SerializeField]
    protected float attackRange = 2f;



    // The player target.
    protected Transform targetPlayer;

    // The player layer.
    protected int playerAttackLayer = 8;

    // Specifies if enemy is dead.
    protected bool enemyIsDead = false;

    [Space(5)]
    [Header("After death values")]
    // The lifetime after death in seconds.
    [SerializeField]
    protected float lifeTimeAfterDeath = 3f;

    [Space(5)]
    [Header("Blood particles")]
    [SerializeField]
    protected GameObject bloodParticle;

    // Animator reference
    protected Animator anim;

    // Event handler for an enemy death.
    public static event EnemyKilledEventHandler EnemyKilled;

    // Represents the reference to the NavMeshAgent.
    protected NavMeshAgent navMeshAgent;

    // The active time of the light
    [Space(5)]
    [Header("Hit light")]
    [SerializeField]
    protected float hitLightTime = 0.2f;

    // Max lifetime of the enemy in seconds.
    [Space(5)]
    [Header("Enemy life time")]
    [Tooltip("The maximum life time of the enemy in seconds.")]
    [SerializeField]
    float maxEnemyLifeTime = 300f;

    [SerializeField]
    private float hitBlinkTime = 0.2f;

    [Space(5)]
    [Header("Death particle burst")]
    [SerializeField]
    private GameObject deathBurst;

    // The Finite state machine.
    protected FSMSystem fsm;

    // Defines if the enemy was killed with a ragdoll method.
    private bool killedWithRagdoll = false;

    // Defines the position of the damagedealer (player, rocket, etc.).
    private Vector3 originRagdollForcePosition;

    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the enemy health.
    /// </summary>
    public int Health
    {
        get
        {
            return this.health;
        }
        set
        {
            if (this.health >= 0 && value <= 0 && !enemyIsDead)
            {
                this.health = value;
                enemyIsDead = true;
                DestroyEnemy();
            }

            if (value >= minHealth && value <= maxHealth)
                this.health = value;
        }
    }

    /// <summary>
    /// Gets or sets max health.
    /// </summary>
    public int MaxHealth
    {
        get { return this.maxHealth; }
        set { this.maxHealth = value; }
    }

    /// <summary>
    /// Gets the melee attack damage.
    /// </summary>
    public int MeleeAttackDamage
    {
        get { return this.meleeAttackDamage; }
        set { this.meleeAttackDamage = value; }
    }

    /// <summary>
    /// Gets the melee attack range.
    /// </summary>
    public float AttackRange
    {
        get { return this.attackRange; }
    }

    /// <summary>
    /// Gets the attack interval.
    /// </summary>
    public float AttackInterval
    {
        get { return this.attackInterval; }
    }

    /// <summary>
    /// Gets the push away force.
    /// </summary>
    public float PushAwayForce
    {
        get { return this.pushAwayForce; }
    }

    public float MovementAnimationSpeed { get { return this.movementAnimationSpeed; } }

    /// <summary>
    /// Gets or sets the canShoot value.
    /// </summary>
    public bool CanShoot
    {
        get { return this.canShoot; }
        set { this.canShoot = value; }
    }

    /// <summary>
    /// Gets or sets the Movement Speed.
    /// </summary>
    public float MovementSpeed
    {
        get { return this.movementSpeed; }
        set
        {
            this.movementSpeed = value;

            if (navMeshAgent != null)
                navMeshAgent.speed = this.movementSpeed;
        }
    }

    /// <summary>
    /// Get initial movement speed.
    /// </summary>
    public float InitialMovementSpeed
    {
        get { return this.initialMovementSpeed; }
    }

    /// <summary>
    /// Gets the target player.
    /// </summary>
    public Transform TargetPlayer
    {
        get { return this.targetPlayer; }
    }

    public Vector3 OriginRagdollForcePosition
    {
        get { return this.originRagdollForcePosition; }
        set { this.originRagdollForcePosition = value; }
    }

    public bool KilledWithRagdoll
    {
        get { return this.killedWithRagdoll; }
    }

    /// <summary>
    /// Gets or Sets the lifetime after death
    /// </summary>
    public float LifeTimeAfterDeath
    {
        get { return this.lifeTimeAfterDeath; }
        set { this.lifeTimeAfterDeath = value; }
    }
    #endregion

    #region Delegates & Events
    public delegate void AttackAheadEventHandler(float currentAttackTime, float maxAttackTime);
    public event AttackAheadEventHandler AttackAhead;

    public delegate void AttackCanceledEventHandler();
    public event AttackCanceledEventHandler AttackCanceled;
    #endregion

    protected virtual void Awake()
    {
        // Register eventhandler
        BasePlayer.PlayerDied += CalculateTargetPlayer;
        BasePlayer.PlayerSpawned += CalculateTargetOnPlayerJoin;

        LevelEndManager.levelExitEvent += ResetValues;
    }

    #region Methods
    // Use this for initialization
	protected virtual void Start ()
    {
        enemyMaterial = transform.GetComponentInChildren<Renderer>().material;
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = movementSpeed;

        initialMovementSpeed = movementSpeed;

        anim = GetComponent<Animator>();

        //Find player
        CalculateTargetPlayer();

        EnemySpawnScaleTween();

        //Init Finite state machine.
        MakeFSM();

        // Invoke enemy death.
        Invoke("InstantKill", maxEnemyLifeTime);
	}

    protected virtual void Update()
    {
        // Update Finite state machine
        if (targetPlayer != null)
        {
            fsm.CurrentState.Reason(targetPlayer.gameObject, this.gameObject);
            fsm.CurrentState.Act(targetPlayer.gameObject, this.gameObject);
        }
        else
        {
            fsm.CurrentState.Reason(null, this.gameObject);
            fsm.CurrentState.Act(null, this.gameObject);
        }
    }

    /// <summary>
    /// Initializes the Finite state machine.
    /// </summary>
    protected virtual void MakeFSM()
    {
        // Follow behaviour
        FollowPlayer follow = new FollowPlayer(attackRange, playerAttackLayer, this);
        follow.AddTransition(Transition.InPlayerAttackRange, StateID.AttackPlayer);
        follow.AddTransition(Transition.ReachedDestination, StateID.Idle);


        // Attack behaviour
        AttackPlayer attack = new AttackPlayer(attackRange, playerAttackLayer, attackInterval, pushAwayForce, this);
        attack.AddTransition(Transition.LostPlayerAttackRange, StateID.FollowPlayer);
        attack.AddTransition(Transition.ReachedDestination, StateID.Idle);

        // Idle behaviour
        IdleEnemy idle = new IdleEnemy();
        idle.AddTransition(Transition.SawPlayer, StateID.FollowPlayer);


        fsm = new FSMSystem();
        fsm.AddState(follow);
        fsm.AddState(attack);
        fsm.AddState(idle);
    }

    /// <summary>
    /// Draws the object some damage and lowers the health.
    /// </summary>
    /// <param name="damage">The damage dealt.</param>
    /// /// <param name="damageDealer">The damage dealer.</param>
    public virtual void TakeDamage(int damage, MonoBehaviour damageDealer)
    {
        // send event if enemy will be dead
        if(Health - damage <= 0 && !enemyIsDead){
            string character = "undefined";

            if (damageDealer != null)
            {
                if (damageDealer is BasePlayer)
                {
                    character = ((BasePlayer)damageDealer).PlayerIdentifier.ToString("g");
                    Instantiate(((BasePlayer)damageDealer).killParticles, transform.position, transform.rotation);
                }
                else
                {
                    character = damageDealer.GetType().Name;
                    Instantiate(deathBurst, transform.position, transform.rotation);
                }
            }

            new Event(Event.TYPE.kill).addPos(this.transform).addCharacter(character).addWave().addEnemy(this.enemyName).addLevel().addPlayerCount().send();
        }

        // Blood particle
        if (bloodParticle != null && damageDealer != null)
            Instantiate(bloodParticle, transform.position, bloodParticle.transform.rotation);

        // Substract health
        if(Health >= 0)
            Health -= damage;

        // Material emmission blink
        if (enemyMaterial != null)
            BlinkMaterial(damageDealer);
    }

    /// <summary>
    /// Deals damage and enables ragdoll (no death animation) if enemy would die from this damage.
    /// </summary>
    /// <param name="damage">Damage</param>
    /// <param name="damageDealer">The damage dealer</param>
    /// <param name="noDeathAnimation">If true: Animator object will be set to null if the damage would kill the enemy.</param>
    public virtual void TakeDamage(int damage, MonoBehaviour damageDealer, Vector3 damageDealerPosition)
    {
        if(Health - damage <= 0)
        {
            originRagdollForcePosition = damageDealerPosition;
            killedWithRagdoll = true;
        }
        TakeDamage(damage, damageDealer);
    }

    /// <summary>
    /// Sets the health instantly to the min health value.
    /// </summary>
    public virtual void InstantKill(MonoBehaviour trigger)
    {
        TakeDamage(MaxHealth, trigger);
    }

    /// <summary>
    /// Tries to set the desired transition.
    /// </summary>
    /// <param name="transition">Transition</param>
    public virtual void SetTransition(Transition transition)
    {
        fsm.PerformTransition(transition);
    }

    /// <summary>
    /// Implements the destruction logic of the enemy.
    /// </summary>
    protected virtual void DestroyEnemy()
    {
        //Disable
        targetPlayer = null;
        GetComponent<NavMeshAgent>().Stop();
        GetComponent<NavMeshAgent>().updateRotation = false;
        GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        GetComponent<Collider>().enabled = false;

        //Animation
        if (anim != null)
        {
            anim.speed = 1f;
            anim.SetBool("Death", true);

            // Normal Scale Fade out.
            LeanTween.scale(gameObject, Vector3.zero, lifeTimeAfterDeath).setEase(LeanTweenType.easeOutQuart).setIgnoreTimeScale(true);
        }

        //Event.
        OnEnemyDeath();

        //Destroy
        Destroy(gameObject, lifeTimeAfterDeath + 0.2f);
    }

    /// <summary>
    /// Calculates the player which should be attacked.
    /// </summary>
    public virtual void CalculateTargetPlayer()
    {
        if (!enemyIsDead)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 1)
                targetPlayer = players[0].transform;
            else if (players.Length > 1)
                targetPlayer = players[Random.Range(0, players.Length)].transform;
            else
            {
                targetPlayer = null;
            }
        }
    }

    /// <summary>
    /// Calculates a new target when a player joins.
    /// A new targt will only be set if the actual target is null;
    /// </summary>
    public virtual void CalculateTargetOnPlayerJoin(BasePlayer player)
    {
        if (targetPlayer == null)
            CalculateTargetPlayer();
    }

    /// <summary>
    /// Sets a new target.
    /// </summary>
    /// <param name="target"></param>
    public virtual void SetNewTarget(Transform target)
    {
        if(target != null)
            this.targetPlayer = target;
    }

    /// <summary>
    /// Ranged attack method.
    /// </summary>
    public virtual void Shoot() { }

    /// <summary>
    /// Melee attack method.
    /// </summary>
    public virtual void Attack()
    {
        if (targetPlayer.GetComponent<MonoBehaviour>() is IDamageable)
        {
            (targetPlayer.GetComponent<MonoBehaviour>() as IDamageable).TakeDamage(MeleeAttackDamage, this);

            if (anim != null)
                anim.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// Event method for enemy death.
    /// </summary>
    protected virtual void OnEnemyDeath()
    {
        if (EnemyKilled != null)
            EnemyKilled(this);
    }

    public virtual void EnemySpawnScaleTween()
    {
        // Spawn scale
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        LeanTween.scale(this.gameObject, originalScale, 0.7f).setEase(AnimCurveContainer.AnimCurve.pingPong);
    }

    protected virtual void BlinkMaterial(MonoBehaviour damageDealer)
    {
        if (damageDealer != null && damageDealer is BasePlayer)
        {
            BasePlayer p = damageDealer as BasePlayer;
            float halfBlinkTime = hitBlinkTime * 0.5f;

            enemyMaterial.SetColor("_GlowEmissionColor", p.PlayerColor);
            LeanTween.value(gameObject, 0f, 1f, halfBlinkTime).setEase(LeanTweenType.easeOutSine)
                .setOnUpdate((float val) => {
                    enemyMaterial.SetFloat("_EffectAmount", val);
                })
                .setOnComplete(() => {
                        LeanTween.value(gameObject, 1f, 0f, halfBlinkTime).setEase(LeanTweenType.easeOutSine)
                   .setOnUpdate((float val) => {
                       enemyMaterial.SetFloat("_EffectAmount", val);
                   });
                });
        }
    }

    /// <summary>
    /// Resets all neccessary values.
    /// </summary>
    protected virtual void ResetValues()
    {
        EnemyKilled = null;
    }

    public void OnAttackAhead(float currentAttackTime, float maxAttackTime)
    {
        if (AttackAhead != null)
            AttackAhead(currentAttackTime, maxAttackTime);
    }

    public void OnAttackCanceled()
    {
        if (AttackCanceled != null)
            AttackCanceled();
    }
    #endregion
}