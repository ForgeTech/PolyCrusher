using UnityEngine;
using System.Collections;

/// <summary>
/// The state for the idle behavior of the chicken.
/// The chicken waits and explodes after some time.
/// </summary>
public class IdleChicken : FSMState 
{
    // Specifies if the destroy action of the chicken behaviour has been activated or not.
    private bool actionPerformed = false;

    public IdleChicken()
    {
        this.stateID = StateID.Idle;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        
    }

    public override void Act(GameObject player, GameObject npc)
    {
        npc.GetComponent<Animator>().SetFloat("MoveValue", 0f);

        if (!actionPerformed)
        {
            if (npc.GetComponent<MonoBehaviour>() is ChickenBehaviour)
            {
               ChickenBehaviour chicken = npc.GetComponent<MonoBehaviour>() as ChickenBehaviour;
               chicken.StartCoroutine(chicken.WaitForExplosion());

                actionPerformed = true;
            }            
        }
    }
}
