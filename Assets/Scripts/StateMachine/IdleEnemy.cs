using UnityEngine;
using System.Collections;

/// <summary>
/// State behaviour for idle.
/// </summary>
public class IdleEnemy : FSMState
{
    public IdleEnemy()
    {
        this.stateID = StateID.Idle;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        if (m is BaseEnemy)
        {
            BaseEnemy e = (BaseEnemy) m;

            if (e.TargetPlayer != null)
                e.SetTransition(Transition.SawPlayer);
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

        if (agent.enabled)
        {
            npc.GetComponent<NavMeshAgent>().Stop();
            npc.GetComponent<NavMeshAgent>().updateRotation = false;
        }

        // Set move value so the idle animation will be played.
        Animator anim = npc.GetComponent<Animator>();

        if (anim != null)
            anim.SetFloat("MoveValue", 0f);
    }
}
