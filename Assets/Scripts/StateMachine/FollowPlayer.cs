using UnityEngine;
using System.Collections;

/// <summary>
/// Implements the Follow player behaviour.
/// </summary>
public class FollowPlayer : FSMState 
{
    // Attack range of the enemy.
    float playerAttackRange = 10f;

    // Layer of the players
    int playerLayer = 8;

    public FollowPlayer(float playerAttackRange, int playerLayer)
    {
        this.playerAttackRange = playerAttackRange;
        this.stateID = StateID.FollowPlayer;
        this.playerLayer = playerLayer;
    }


    public override void Reason(GameObject player, GameObject npc)
    {
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();
        BaseEnemy e = null;

        if (m is BaseEnemy)
            e = (BaseEnemy)m;

        if (player != null && npc != null)
        {
            RaycastHit hitInfo;
            bool hit = CheckAttackRange(player, npc, out hitInfo);

            // If the player is in attack range, make a transition to attack.
            if (hit && hitInfo.transform.gameObject.tag == "Player" && e != null)
            {
                // Set animator speed back to 1.
                Animator anim = npc.GetComponent<Animator>();
                anim.speed = 1;

                e.SetTransition(Transition.InPlayerAttackRange);
            }
        }

        // Change to idle if player is null
        if (player == null || e.TargetPlayer == null)
        {
            //Debug.Log("FollowPlayer: Transition to idle!");
            e.SetTransition(Transition.ReachedDestination);
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

        if (agent.enabled)
        {
            agent.updateRotation = true;
            agent.Resume();
            agent.SetDestination(player.transform.position);
        }

        // Animator settings
        Animator anim = npc.GetComponent<Animator>();
        float moveValue = (agent.desiredVelocity).magnitude / npc.GetComponent<BaseEnemy>().InitialMovementSpeed;

        //Debug.Log(moveValue);
        if (anim != null)
        {
            anim.speed = moveValue;
            anim.SetFloat("MoveValue", moveValue);
        }
    }

    /// <summary>
    /// Checks the attack range of the enemy and returns true if it is in range.
    /// </summary>
    /// <param name="player">Player Gameobject</param>
    /// <param name="npc">NPC</param>
    /// <returns>True: In range, False: Not in range.</returns>
    private bool CheckAttackRange(GameObject player, GameObject npc, out RaycastHit hitInfo)
    {
        Vector3 pPos = new Vector3(player.transform.position.x, 1f, player.transform.position.z);
        Vector3 nPos = new Vector3(npc.transform.position.x, 1f, npc.transform.position.z);
        Ray ray = new Ray(nPos, (pPos - nPos).normalized);

        Debug.DrawRay(nPos, (pPos - nPos).normalized * playerAttackRange, Color.green);

        return Physics.Raycast(ray, out hitInfo, playerAttackRange, 1 << playerLayer);
    }
}
