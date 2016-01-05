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

    // Signals if the attack has been started.
    protected bool attackStarted;

    public BossAttackMelee(float phaseTime, StateID id)
    {
        this.stateID = id;
        this.phaseTime = phaseTime;
        this.currentPhaseTime = this.phaseTime;

        this.attackAllowed = false;
        this.currentAttackTimer = 0f;
        this.attackStarted = false;
    }

    /// <summary>
    /// Actions of the State.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Act(GameObject player, GameObject npc)
    {
        if (currentPhaseTime > 0)
        {
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

            // Stop the nav agent while attacking.
            if (agent != null && agent.enabled)
                agent.Stop();


            // Attack logic.
            if (player != null && m is BossEnemy)
            {
                BossEnemy e = (BossEnemy)m;
                AttackPlayer(e);
            }
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

            // Phase time logic
            if (CheckCurrentPhaseTime(e))
                return;

            // Only make walk transition if player is not dead.
            if (player != null && currentPhaseTime > 0)
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
                attackStarted = false;
                attackAllowed = false;
                this.currentAttackTimer = 0f;
                return;
            }

            //Debug.Log("PhaseTime: " + currentPhaseTime);
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
    }

    /// <summary>
    /// Checks the range to the player and returns true if the player is in range and false
    /// if the player is out of range.
    /// </summary>
    /// <param name="e">Reference to the boss enemy.</param>
    /// <returns>True: Player is in range, False: Player is not in range.</returns>
    protected virtual bool CheckAttackRange(BossEnemy e)
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
                    if (e.TargetPlayer.GetComponent<BasePlayer>() != null)
                    {
                        // If the names aren't equal there is no hit.
                        if (((BasePlayer)m).PlayerName != e.TargetPlayer.GetComponent<BasePlayer>().PlayerName)
                            hit = false;
                    }
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
    protected virtual bool CheckCurrentPhaseTime(BossEnemy e)
    {
        // Increase current time.
        currentPhaseTime -= Time.deltaTime;

        // If the current phase time is finished, go do Idle.
        if (currentPhaseTime <= 0f)
        {
            e.SetTransition(Transition.AttackFinished);
            attackStarted = false;
            attackAllowed = false;
            this.currentAttackTimer = 0f;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attack logic of the boss.
    /// </summary>
    /// <param name="e">Boss reference</param>
    protected virtual void AttackPlayer(BossEnemy e)
    {
        // Attack only if allowed.
        if (attackAllowed && !attackStarted)
        {
            //Debug.Log("Attack!!! Pew Pew Pew!");

            //Play animation.
            Animator anim = e.GetComponent<Animator>();

            if (anim != null)
                anim.SetTrigger("Melee");

            attackStarted = true;

            GameObject areaOfDamageReference = GameObject.Instantiate(e.MeleeAreaOfDamage, e.TargetPlayer.position, e.MeleeAreaOfDamage.transform.rotation) as GameObject;
            areaOfDamageReference.GetComponent<BossMeleeScript>().InitMeleeScript(e.AreoOfDamageRadius, e.AreaOfDamageTime, e, e.MeleeAttackDamage);

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
