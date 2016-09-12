using UnityEngine;

/// <summary>
/// Implements the Follow player behaviour.
/// </summary>
public class FollowPlayer : FSMState 
{
    // Attack range of the enemy.
    float playerAttackRange = 10f;

    // Layer of the players
    int playerLayer = 8;

    private readonly BaseEnemy enemy;
    private readonly Animator animator;

    public FollowPlayer(float playerAttackRange, int playerLayer, BaseEnemy enemy)
    {
        this.playerAttackRange = playerAttackRange;
        this.stateID = StateID.FollowPlayer;
        this.playerLayer = playerLayer;
        this.enemy = enemy;
        this.animator = enemy.GetComponent<Animator>();
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        if (player != null && npc != null)
        {
            //RaycastHit hitInfo;
            //bool hit = CheckAttackRange(player, npc, out hitInfo);
            bool hit = IsPlayerNearEnough(player);

            // If the player is in attack range, make a transition to attack.
            if (hit && enemy != null)
            {
                // Set animator speed back to 1.
                animator.speed = 1;
                enemy.SetTransition(Transition.InPlayerAttackRange);
            }
        }

        // Change to idle if player is null
        if (player == null || enemy.TargetPlayer == null)
        {
            //Debug.Log("FollowPlayer: Transition to idle!");
            enemy.SetTransition(Transition.ReachedDestination);
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        
        if (agent.enabled && !agent.pathPending)
        {
            agent.updateRotation = true;
            agent.Resume();

            agent.SetDestination(player.transform.position);
        }

        // Animator settings
        float moveValue = (agent.desiredVelocity).magnitude / npc.GetComponent<BaseEnemy>().InitialMovementSpeed;

        //Debug.Log("Move Value: " + moveValue);
        if (animator != null)
        {
            if (!agent.pathPending)
            {
                animator.speed = moveValue * enemy.MovementAnimationSpeed;
                animator.SetFloat("MoveValue", moveValue);
            }
            else
            {
                animator.speed = 1f;
                animator.SetFloat("MoveValue", 1.0f);
            }
        }
    }

    public override void DoBeforeEntering()
    {
        animator.speed = 1;
    }

    public override void DoBeforeLeaving()
    {
        animator.speed = 1;
    }

    private bool IsPlayerNearEnough(GameObject player)
    {
        Vector3 playerPos = player.transform.position + Vector3.up;
        Vector3 enemyPos = enemy.transform.position + Vector3.up;

#if UNITY_EDITOR
        Debug.DrawRay(enemyPos, (playerPos - enemyPos).normalized * playerAttackRange, Color.green);
#endif

        return Vector3.Distance(playerPos, enemyPos) <= playerAttackRange;
    }
}
