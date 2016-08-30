using UnityEngine;
using System.Collections;

/// <summary>
/// The chicken searches attracts enemies onto itself and lures them to a random position.
/// After some time the chicken stops and explodes after some time and damages the enemies in range.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ChickenBehaviour : MonoBehaviour 
{
    // The radius where the enemys will be attracted.
    [SerializeField]
    protected float attractEnemyRadius = 6f;

    // The radius in which the random point will be calculated.
    [SerializeField]
    protected float targetPositionRadius = 15f;

    // The radius where the enemies get damage.
    [SerializeField]
    protected float attackRadius = 6f;

    // The damage of the explosion.
    [SerializeField]
    protected int damage = 100;

    // The random target position of the chicken.
    protected Vector3 targetPosition;

    // Determines if the target has been reached.
    protected bool hasReachedTarget = false;

    // The time to wait until the chicken explodes.
    [SerializeField]
    protected float waitForExplosion = 2f;

    [Header("Explosion particle")]
    [SerializeField]
    protected GameObject explosionParticle;

    [Header("Explosion sound")]
    [SerializeField]
    protected AudioClip explosionSound;

    // Reference of the animator component.
    protected Animator anim;

    // The Finite state machine.
    protected FSMSystem fsm;

    // The enemies which are influenced by the chicken.
    protected Transform[] influencedEnemies;

    // The owner of the projectile.
    protected MonoBehaviour ownerScript;

    protected RumbleManager rumbleManager;


    /// <summary>
    /// Gets the target position.
    /// </summary>
    public Vector3 TargetPosition
    {
        get { return this.targetPosition; }
    }

    /// <summary>
    /// Gets or sets has reached target.
    /// </summary>
    public bool HasReachedTarget
    {
        get { return this.hasReachedTarget; }
        set { this.hasReachedTarget = value; }
    }

    /// <summary>
    /// Gets or sets the owner script.
    /// </summary>
    public MonoBehaviour OwnerScript
    {
        get { return this.ownerScript; }
        set { this.ownerScript = value; }
    }

    /// <summary>
    /// Sets the rumble manager for accessing rumble functions
    /// </summary>
    public RumbleManager RumbleManager
    {
        set { rumbleManager = value; }
    }

    // Use this for initialization
    void Start () 
    {
        // Animator
        anim = GetComponent<Animator>();

        // Set new target for the enemies in range.
        SetEnemyTarget();

        // Calculate random point on navmesh.
        //CalculateRandomPoint();
        CalculateTargetPoint();

        // Init state machine.
        MakeFSM();

        // Scale tween
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        LeanTween.scale(this.gameObject, originalScale, 0.9f).setEase(AnimCurveContainer.AnimCurve.pingPong);
	}

    void FixedUpdate()
    {
        fsm.CurrentState.Reason(null, this.gameObject);
        fsm.CurrentState.Act(null, this.gameObject);
    }

    /// <summary>
    /// Initializes the Finite state machine.
    /// </summary>
    protected virtual void MakeFSM()
    {
        WalkChicken walk = new WalkChicken(targetPosition);
        walk.AddTransition(Transition.ReachedDestination, StateID.Idle);

        IdleChicken idle = new IdleChicken();
        idle.AddTransition(Transition.Walking, StateID.WalkTowardsTarget);

        fsm = new FSMSystem();
        fsm.AddState(walk);
        fsm.AddState(idle);
    }

    /// <summary>
    /// Calculates a random point on the NavMesh and saves it to the targetPosition.
    /// </summary>
    protected void CalculateRandomPoint()
    {
        NavMeshHit hit;

        // Save random direction.
        Vector3 direction = Random.insideUnitSphere;

        // Calc random direction only on the normalized XZ-Plane.
        Vector3 randomDir = new Vector3(direction.x, 0, direction.z).normalized * targetPositionRadius;

        // Set the random direction to the actual transform position.
        randomDir += transform.position;

        //Calculate the actual position (this is necessary because the random point could be outside of the navmesh).
        NavMesh.SamplePosition(randomDir, out hit, targetPositionRadius, NavMesh.AllAreas);

        //Set the target position.
        targetPosition = hit.position;
    }

    /// <summary>
    /// Calculates the target point of the chicken.
    /// The target point is the forward vector of the player.
    /// </summary>
    protected void CalculateTargetPoint()
    {
        NavMeshHit hit;

        
        NavMesh.SamplePosition(transform.position + transform.forward * targetPositionRadius, out hit, targetPositionRadius, NavMesh.AllAreas);
        
        targetPosition = hit.position;
    }

    /// <summary>
    /// Waits before the explosion procedure starts.
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForExplosion()
    {
        // Death animation.
        anim.SetBool("Death", true);

        yield return new WaitForSeconds(waitForExplosion);

        Rumble();
        PerformExplosionProcedure();
    }


    protected void Rumble()
    {
        if (rumbleManager != null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, 1 << 8);
            BasePlayer basePlayer;
            for (int i = 0; i < hits.Length; i++)
            {
                basePlayer = hits[i].GetComponent<BasePlayer>();
                if (basePlayer != null)
                {
                    rumbleManager.Rumble(basePlayer.InputDevice, RumbleType.BasicRumbleLong);
                }
                else
                {
                    Debug.Log("No Baseplayer found for rumbling!");
                }
            }
        }
    }

    /// <summary>
    /// Explodes the chicken, and damages all enemies in the specified radius.
    /// </summary>
    protected void PerformExplosionProcedure()
    {
        Transform[] enemies = GetAllEnemiesInRange(attackRadius);

        foreach (Transform enemy in enemies)
        {
            if (enemy.GetComponent<MonoBehaviour>() is BaseEnemy)
            {
                BaseEnemy e = (enemy.GetComponent<MonoBehaviour>() as BaseEnemy);

                // Deal damage to the enemy
                e.TakeDamage(damage, this, transform.position);
            }
        }

        // Calculate new target if enemy does not die.
        foreach(Transform enemy in influencedEnemies)
        {
            if (enemy != null && enemy.GetComponent<MonoBehaviour>() is BaseEnemy)
            {
                BaseEnemy e = (enemy.GetComponent<MonoBehaviour>() as BaseEnemy);

                if (e.Health > 0)
                    e.CalculateTargetPlayer();
            }
        }

        // Explosion
        if (explosionParticle != null)
        {
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
        }
        
        // Sound
        if (explosionSound != null)
        {
            SoundManager.SoundManagerInstance.Play(explosionSound, transform.position);
        }

        // Camera shake
        CameraManager.CameraReference.ShakeOnce();

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Sets the enemy target to the chicken position.
    /// </summary>
    protected void SetEnemyTarget()
    {
        influencedEnemies = GetAllEnemiesInRange(attractEnemyRadius);

        foreach (Transform enemy in influencedEnemies)
        {
            if(enemy.GetComponent<MonoBehaviour>() is BaseEnemy)
            {
                (enemy.GetComponent<MonoBehaviour>() as BaseEnemy).SetNewTarget(transform);
            }
        }
    }

    /// <summary>
    /// Returns all enemy transforms in the given range.
    /// </summary>
    /// <param name="range">Range</param>
    /// <returns></returns>
    protected Transform[] GetAllEnemiesInRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, 1 << 9); // Only Enemies
        Transform[] enemies = new Transform[hits.Length];

        for (int i = 0; i < hits.Length; i++)
            enemies[i] = hits[i].transform;

        return enemies;
    }

    /// <summary>
    /// Tries to set the desired transition.
    /// </summary>
    /// <param name="transition">Transition</param>
    public virtual void SetTransition(Transition transition)
    {
        fsm.PerformTransition(transition);
    }
}
