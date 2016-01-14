using UnityEngine;
using System.Collections;
using System;

public class BossWalk : FSMState
{
    // Layer of the players
    protected int playerLayer = 8;

    // Decection range
    protected float detectionRange;

    public BossWalk(float detectionRange, StateID stateID)
    {
        this.stateID = stateID;
        this.detectionRange = detectionRange;
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

        Animator anim = npc.GetComponent<Animator>();

        if (anim != null)
        { 
            anim.SetTrigger("Move");
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

            // If the boss is in attack range again, make a transition back.
            if (player != null && CheckAttackRange(e))
                e.SetTransition(Transition.ReachedDestination);

            // If thera are no players alive -> Transition back.
            if (player == null)
                e.SetTransition(Transition.ReachedDestination);
        }    
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        base.DoBeforeEntering();

        //Debug.Log("Boss: Walk State");
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();
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
            bool hit = Physics.Raycast(ray, out hitInfo, detectionRange, 1 << playerLayer);

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
            Debug.DrawRay(enemyPos, (playerPos - enemyPos).normalized * detectionRange, Color.green);

            return hit;
        }
        return false;
    }
}
