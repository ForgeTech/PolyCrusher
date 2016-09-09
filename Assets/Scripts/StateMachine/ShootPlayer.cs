using UnityEngine;
using System.Collections;

/// <summary>
/// State behaviour for the shooting attack.
/// </summary>
public class ShootPlayer : AttackPlayer
{
    public ShootPlayer(float playerAttackRange, int playerLayer, float attackInterval, BaseEnemy enemy)
        : base(playerAttackRange, playerLayer, attackInterval, 0f, enemy)
    {
        
    }

    public override void Act(GameObject player, GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

        if (agent.enabled)
        {
            agent.Stop();
            agent.updateRotation = true;
        }
        

        // ========Animator settings ========
        Animator anim = npc.GetComponent<Animator>();

        if (anim != null)
            anim.SetFloat("MoveValue", 0f);
        //===================================

        //Rotate npc
        if(player != null)
            npc.transform.rotation = Quaternion.LookRotation((player.transform.position - npc.transform.position).normalized, Vector3.up);

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

            if (p is IDamageable && n is BaseEnemy)
                (n as BaseEnemy).Attack();

            attack = false;
        }
        currentAttacktime += Time.deltaTime;
    }
}
