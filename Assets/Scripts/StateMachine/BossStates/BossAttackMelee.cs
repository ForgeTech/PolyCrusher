using UnityEngine;
using System.Collections;
using System;

public class BossAttackMelee : FSMState
{
    // Layer of the players
    protected int playerLayer = 8;

    // Current phase timer.
    protected float currentPhaseTime = 0f;

    // The actual phase time.
    protected float phaseTime;

    // Specifies if the attack is allowed or not.
    protected bool attackAllowed;

    // Attack interval timer.
    protected float currentAttackTimer;

    // Specifies if a state is already changed or not.
    protected bool stateChanged;

    // Reference to the instantiated area of damage prefab.
    protected GameObject areaOfDamageReference;

    // Signals if the attack has been started.
    protected bool attackStarted;

    public BossAttackMelee(float phaseTime)
    {
        this.stateID = StateID.BossAttackMelee;
        this.phaseTime = phaseTime;
        this.currentPhaseTime = this.phaseTime;

        this.attackAllowed = false;
        this.currentAttackTimer = 0f;
        this.stateChanged = false;
        this.attackStarted = false;
    }

    /// <summary>
    /// Actions of the State.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Act(GameObject player, GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        // Stop the nav agent while attacking.
        if (agent != null && agent.enabled)
            agent.Stop();

        // Attack logic.
        if (player != null && m is BossEnemy)
        {
            BossEnemy e = (BossEnemy) m;
            AttackPlayer(e);
        }
        
    }

    /// <summary>
    /// Reason for a transition to another state.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Reason(GameObject player, GameObject npc)
    {
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        if (m is BossEnemy)
        {

            BossEnemy e = (BossEnemy)m;

            // Only make walk transition if player is not dead.
            if (player != null)
            {
                if (!CheckAttackRange(e))
                {
                    // Transition to walk
                    e.SetTransition(Transition.LostPlayerAttackRange);
                }
            }
            else
            {
                // Back to idle if all players are dead.
                e.SetTransition(Transition.AttackFinished);
                return;
            }

            // Phase Time Logic
            CheckCurrentPhaseTime(e);
            
        }
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        base.DoBeforeEntering();

        Debug.Log("Boss: Melee Attack State");

        if (currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();

        if(currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;

        stateChanged = false;
        attackStarted = false;
        //currentAttackTimer = 0f;
    }

    /// <summary>
    /// Checks the range to the player and returns true if the player is in range and false
    /// if the player is out of range.
    /// </summary>
    /// <param name="e">Reference to the boss enemy.</param>
    /// <returns>True: Player is in range, False: Player is not in range.</returns>
    private bool CheckAttackRange(BossEnemy e)
    {
        if (e != null && e.TargetPlayer != null)
        {
            RaycastHit hitInfo;

            Vector3 playerPos = new Vector3(e.TargetPlayer.position.x, e.TargetPlayer.position.y + 1f, e.TargetPlayer.position.z);
            Vector3 enemyPos = new Vector3(e.transform.position.x, e.transform.position.y + 1f, e.transform.position.z);
            Ray ray = new Ray(enemyPos, (playerPos - enemyPos).normalized);

            // Raycast hit check
            bool hit = Physics.Raycast(ray, out hitInfo, e.AttackRange, 1 << playerLayer);

            // Check if the payer target equals the collided target.
            if (hit)
            {
                MonoBehaviour m = hitInfo.transform.GetComponent<MonoBehaviour>();

                if (m != null && m is BasePlayer)
                {
                    // If the names aren't equal there is no hit.
                    if (((BasePlayer)m).PlayerName != e.TargetPlayer.GetComponent<BasePlayer>().PlayerName)
                        hit = false;
                }
            }

            // Debug draw Ray
            Debug.DrawRay(enemyPos, (playerPos - enemyPos).normalized * e.AttackRange, Color.yellow);

            return hit;
        }
        return false;
    }

    /// <summary>
    /// Timer logic for the phase time.
    /// Decreases the timer and makes the transition if it reaches its endpoint.
    /// </summary>
    private void CheckCurrentPhaseTime(BossEnemy e)
    {
        // Increase current time.
        currentPhaseTime -= Time.deltaTime;

        // If the current phase time is finished, go do Idle.
        if (currentPhaseTime <= 0f)
        {
            e.SetTransition(Transition.AttackFinished);
        }
    }

    /// <summary>
    /// Attack logic of the boss.
    /// </summary>
    /// <param name="e">Boss reference</param>
    private void AttackPlayer(BossEnemy e)
    {
        // Attack only if allowed.
        if (attackAllowed)
        {
            Debug.Log("Attack!!! Pew Pew Pew!");

            areaOfDamageReference = GameObject.Instantiate(e.MeleeAreaOfDamage, e.TargetPlayer.position, e.MeleeAreaOfDamage.transform.rotation) as GameObject;
            areaOfDamageReference.GetComponent<BossMeleeScript>().InitMeleeScript(e.AreoOfDamageRadius, e.AttackInterval, e, e.MeleeAttackDamage);

            attackAllowed = false;
        }

        // Timer logic.
        if (currentAttackTimer >= e.AttackInterval)
        {
            attackAllowed = true;
            currentAttackTimer = 0f;
        }

        // Increase attack timer.
        currentAttackTimer += Time.deltaTime;
    }
}
