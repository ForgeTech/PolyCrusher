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
    protected override void DestroyEnemy()
    {
        //Disable
        targetPlayer = null;
        GetComponent<NavMeshAgent>().Stop();
        GetComponent<NavMeshAgent>().updateRotation = false;
        GetComponent<NavMeshAgent>().obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        //Animation
        if (anim != null)
            anim.SetBool("Death", true);

        //Event.
        OnBossMobKilled();

        //Scale Fade out
        StartCoroutine(transform.ScaleFrom(Vector3.zero, lifeTimeAfterDeath, AnimCurveContainer.AnimCurve.downscale.Evaluate));

        //Destroy
        Destroy(this.gameObject, lifeTimeAfterDeath);
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
