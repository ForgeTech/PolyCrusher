using UnityEngine;
using System.Collections;

/// <summary>
/// The state for the walk towards the random target.
/// </summary>
public class WalkChicken : FSMState 
{
    // The target position.
    protected Vector3 targetPosition;
    
    // Reference to the NavMeshAgent of the npc.
    private NavMeshAgent agent;

    public WalkChicken(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        this.stateID = StateID.WalkTowardsTarget;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        // If the agent exists, check if the npc reached it's destination.
        if (agent != null)
        {
            float distance = agent.remainingDistance;

            // Check if the destination has been reached.
            if (!agent.pathPending && distance != Mathf.Infinity && agent.pathStatus == NavMeshPathStatus.PathComplete && distance == 0)
            { 
                if( npc.GetComponent<MonoBehaviour>() is ChickenBehaviour)
                {
                    (npc.GetComponent<MonoBehaviour>() as ChickenBehaviour).HasReachedTarget = true;
                    (npc.GetComponent<MonoBehaviour>() as ChickenBehaviour).SetTransition(Transition.ReachedDestination);
                }
            }
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        agent = npc.GetComponent<NavMeshAgent>();
        agent.SetDestination(targetPosition);

        npc.GetComponent<Animator>().SetFloat("MoveValue", 1f);
    }
}
