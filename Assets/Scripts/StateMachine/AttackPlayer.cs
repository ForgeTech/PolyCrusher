using UnityEngine;
using System.Collections;

/// <summary>
/// Implements the attacing behaviour if the player is in range.
/// </summary>
public class AttackPlayer : FSMState 
{
    // The attack interval in seconds.
    protected float attackInterval = 0.8f;

    // Current attack time.
    protected float currentAttacktime = 0.0f;

    // Specifies if an attack is allowed.
    protected bool attack;

    // Attack range of the enemy.
    protected float playerAttackRange = 10f;

    // Layer of the players
    protected int playerLayer = 8;

    // Explosion force
    protected float explosionForce;

    // Reference to the coroutine to end the routine.
    protected IEnumerator coroutineAttack;

    // Reference for the first attack coroutine.
    protected IEnumerable coroutineFirstAttack;

    private BaseEnemy enemy;


    public AttackPlayer(float playerAttackRange, int playerLayer, float attackInterval, float explosionForce, BaseEnemy enemy)
    {
        this.stateID = StateID.AttackPlayer;
        this.playerAttackRange = playerAttackRange;
        this.playerLayer = playerLayer;
        this.attackInterval = attackInterval;
        this.explosionForce = explosionForce;
        this.enemy = enemy;

        attack = false;

        // Init and start coroutine the first time.
        coroutineAttack = WaitForNextAttack();
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        if (player != null && npc != null)
        {
            RaycastHit hitInfo;
            Vector3 pPos = new Vector3(player.transform.position.x, 1f, player.transform.position.z);
            Vector3 nPos = new Vector3(npc.transform.position.x, 1f, npc.transform.position.z);
            Ray ray = new Ray(nPos, (pPos - nPos).normalized);

            bool hit = Physics.Raycast(ray, out hitInfo, playerAttackRange, 1 << playerLayer);

            Debug.DrawRay(nPos, (pPos - nPos).normalized * playerAttackRange, Color.yellow);

            // If the player is in attack range, make a transition to walk.
            if (!(hit && hitInfo.transform.gameObject.tag == "Player"))
                npc.GetComponent<BaseEnemy>().SetTransition(Transition.LostPlayerAttackRange);
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

        if (agent.enabled)
        {
            agent.Stop();
        }

        // ========Animator settings ========
        Animator anim = npc.GetComponent<Animator>();

        if (anim != null)
            anim.SetFloat("MoveValue", 0f);
        //===================================


        // Timer
        if (currentAttacktime >= attackInterval)
        {
            attack = true;
            currentAttacktime = 0f;
        }

        if (attack && player != null)
        {
            MonoBehaviour p = player.GetComponent<MonoBehaviour>();
            MonoBehaviour n = npc.GetComponent<MonoBehaviour>();
            
            if (p is IDamageable)
            {
                if (n is BaseEnemy)
                {
                    (n as BaseEnemy).Attack();
                    Vector3 attackPos = new Vector3(npc.transform.position.x, npc.transform.position.y + 0.5f, npc.transform.position.z);
                    player.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, attackPos, 5f, 0f, ForceMode.Impulse);
                }
            }

            attack = false;
        }

        currentAttacktime += Time.deltaTime;
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();
        enemy.OnAttackCanceled();
        attack = false;
        currentAttacktime = 0f;
    }

    public override void DoBeforeEntering()
    {
        base.DoBeforeEntering();
        enemy.OnAttackAhead(currentAttacktime, attackInterval);
        attack = false;
        currentAttacktime = 0f;
    }

    /// <summary>
    /// Waits for the next attack.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator WaitForNextAttack()
    {
        Debug.Log("Coroutine started! - " + attack);
        yield return new WaitForSeconds(attackInterval);
        attack = true;
    }
}
