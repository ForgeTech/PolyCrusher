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
    }
    #endregion

    #region Member variables
    [Space(5)]
    [Header("Range attack values")]

    [SerializeField]
    protected float rangedAttackDamage = 10f;

    [SerializeField]
    protected float rangedAttackInterval = 0.5f;

    [SerializeField]
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
    [SerializeField]
    protected bool sprintEnabled = false;

    [Space(4)]
    [SerializeField]
    protected bool mobSpawnEnabled = false;

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

        BossAttackMelee attackMelee = new BossAttackMelee();
        attackMelee.AddTransition(Transition.AttackFinished, StateID.BossIdle);



        fsm = new FSMSystem();
        fsm.AddState(idle);
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
