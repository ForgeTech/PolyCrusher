using UnityEngine;
using System.Collections;
using System;

public class BossSprint : FSMState
{
    // Specifies if the attack is finished or not.
    protected bool attackFinished;

    // Layer of the players
    protected int playerLayer = 8;

    // Reference to boss enemy
    protected BossEnemy bossEnemy;

    public BossSprint(StateID id, BossEnemy e)
    {
        this.stateID = id;

        this.attackFinished = false;

        this.bossEnemy = e;
    }

    /// <summary>
    /// Actions of the State.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Act(GameObject player, GameObject npc)
    {
        // Reset navigation settings. =========================
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

        if (player != null && agent.enabled)
        {
            agent.updateRotation = true;
            agent.Resume();
            agent.SetDestination(player.transform.position);
        }
        // ===================================================

        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        if (m != null && m is BossEnemy)
        {
            BossEnemy e = (BossEnemy)m;

            //Play animation.
            Animator anim = e.GetComponent<Animator>();

            if (anim != null)
                anim.SetTrigger("Sprint");

            if (CheckAttackRange(e))
            {
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

            // Transition to Idle if the attack was performed.
            if (attackFinished)
                e.SetTransition(Transition.ReachedDestination);
        }
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        base.DoBeforeEntering();

        //Debug.Log("Boss: Sprint Attack State");
        attackFinished = false;

        ParticleSystem[] p = bossEnemy.TurbineParticles;
        for (int i = 0; i < p.Length; i++)
            p[i].Play();
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();

        attackFinished = false;

        ParticleSystem[] p = bossEnemy.TurbineParticles;
        for (int i = 0; i < p.Length; i++)
            p[i].Stop();
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
                    if (e.TargetPlayer != null && e.TargetPlayer.GetComponent<BasePlayer>() != null)
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
    /// Attacks the player.
    /// </summary>
    /// <param name="e">Boss enemy reference.</param>
    protected void AttackPlayer(BossEnemy e)
    {
        MonoBehaviour m = e.TargetPlayer.GetComponent<MonoBehaviour>();

        if (m is BasePlayer)
        {
            BasePlayer p = (BasePlayer)m;

            // Take Damage
            p.TakeDamage(e.MeleeAttackDamage, e);

            // Camera shake
            CameraManager.CameraReference.ShakeOnce();

            // Add force to the player.
            Rigidbody rigid = p.GetComponent<Rigidbody>();

            if (rigid != null)
            {
                // Add attack force.
                rigid.AddExplosionForce(e.PushAwayForce, e.transform.position, e.AttackRange * 1.5f, 0f, ForceMode.Impulse);
            }

            // Reset current damage counter of the boss.
            e.SprintPhase.currentDamage = 0;
            attackFinished = true;
        }
    }
}