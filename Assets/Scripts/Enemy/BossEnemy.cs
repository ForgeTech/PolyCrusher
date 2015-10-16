using UnityEngine;
using System.Collections;

/// <summary>
/// Event handler for a killed boss.
/// </summary>
/// <param name="boss">Boss who died.</param>
public delegate void BossKilledEventHandler(BossEnemy boss);

public class BossEnemy : BaseEnemy
{
    #region Inner Class
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
    #endregion

    #region Member variables
    [Space(5)]
    [Header("Range attack values")]

    [SerializeField]
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

    [Space(4)]
    [Tooltip("Specifies if the boss is able to sprint or not.")]
    [SerializeField]
    protected bool sprintEnabled = false;

    [Space(4)]
    [Tooltip("Specifies if mobs can be spawned by the boss or not.")]
    [SerializeField]
    protected bool mobSpawnEnabled = false;

    [Space(4)]
    [Tooltip("Time which the boss stays in idle after he chooses another state.")]
    [SerializeField]
    protected float idleTime = 2f;

    [Space(5)]
    [Header("Attack visualization and settings")]
    [SerializeField]
    protected GameObject meleeAreaOfDamage;

    [SerializeField]
    [Tooltip("The damage radius of the attack.")]
    protected float areoOfDamageRadius = 5f;

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
    /// Gets sprint enabled.
    /// </summary>
    public bool SprintEnabled
    {
        get { return this.sprintEnabled; }
    }

    /// <summary>
    /// Gets mob spawn enabled.
    /// </summary>
    public bool MobSpawnEnabled
    {
        get { return this.mobSpawnEnabled; }
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
    /// Gets the area of damage radius.
    /// </summary>
    public float AreoOfDamageRadius
    {
        get { return this.areoOfDamageRadius; }
    }
    #endregion

    // Event for a killed boss.
    public static event BossKilledEventHandler BossKilled;

    /// <summary>
    /// Initializes the FSM.
    /// </summary>
    protected override void MakeFSM()
    {
        // Set upt FSM.
        BossIdle idle = new BossIdle();
        idle.AddTransition(Transition.DecisionMelee, StateID.BossAttackMelee);

        BossAttackMelee attackMelee = new BossAttackMelee(MeleePhase.phaseTime);
        attackMelee.AddTransition(Transition.AttackFinished, StateID.BossIdle);
        attackMelee.AddTransition(Transition.LostPlayerAttackRange, StateID.BossWalk);
        
        BossWalk attackPhaseWalk = new BossWalk();
        attackPhaseWalk.AddTransition(Transition.ReachedDestination, StateID.BossAttackMelee);

        fsm = new FSMSystem();
        fsm.AddState(idle);
        fsm.AddState(attackMelee);
        fsm.AddState(attackPhaseWalk);
    }

    /// <summary>
    /// Melee Attack behaviour.
    /// </summary>
    public override void Attack()
    {
        
    }

    /// <summary>
    /// Ranged Attack behaviour.
    /// </summary>
    public override void Shoot()
    {
        
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
}
