using UnityEngine;
using System.Collections;

/// <summary>
/// Kill event handler for the moss mob
/// </summary>
/// <param name="mob">The mob which was killed.</param>
public delegate void BossMobKilledEventHandler(BossMob mob);

/// <summary>
/// Mob enemy type.
/// </summary>
public class BossMob : BaseEnemy
{
    // Kill event
    public static event BossMobKilledEventHandler BossMobKilled;

    /// <summary>
    /// Destroys the enemy.
    /// </summary>
    protected override void DestroyEnemy(bool destroyWithEffects)
    {
        //Disable
        targetPlayer = null;
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.Stop();
        agent.updateRotation = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        if (destroyWithEffects)
        {
            //Animation
            if (anim != null)
                anim.SetBool("Death", true);

            //Scale Fade out
            LeanTween.scale(gameObject, Vector3.zero, lifeTimeAfterDeath).setEase(LeanTweenType.easeOutQuart);
        }
        
        //Event.
        OnBossMobKilled();

        //Destroy
        if (destroyWithEffects)
            Destroy(gameObject, lifeTimeAfterDeath);
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Event method for the Boss kill event.
    /// </summary>
    protected virtual void OnBossMobKilled()
    {
        if (BossMobKilled != null)
            BossMobKilled(this);
    }

    /// <summary>
    /// Resets the boss event handler.
    /// </summary>
    protected override void ResetValues()
    {
        base.ResetValues();
        BossMobKilled = null;
    }
}
