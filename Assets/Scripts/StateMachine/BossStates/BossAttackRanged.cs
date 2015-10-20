using UnityEngine;
using System.Collections;
using System;

public class BossAttackRanged : FSMState
{
    // Layer of the players
    protected int playerLayer = 8;

    // Current phase timer.
    protected float currentPhaseTime = 0f;

    // The actual phase time.
    protected float phaseTime;

    // Specifies if the attack is allowed or not.
    protected bool attackAllowed;

    // Attack interval timer.
    protected float currentAttackTimer;

    // Signals if the attack has been started.
    protected bool attackStarted;

    public BossAttackRanged(float phaseTime)
    {
        this.stateID = StateID.BossAttackRanged;
        this.phaseTime = phaseTime;
        this.currentPhaseTime = this.phaseTime;

        this.attackAllowed = false;
        this.currentAttackTimer = 0f;
        this.attackStarted = false;
    }

    /// <summary>
    /// Actions of the State.
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="npc">NPC reference</param>
    public override void Act(GameObject player, GameObject npc)
    {
        if (currentPhaseTime > 0)
        {
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

            // Stop the nav agent while attacking.
            if (agent != null && agent.enabled)
            {
                agent.Stop();
            }


            // Attack logic.
            if (player != null && m is BossEnemy)
            {
                BossEnemy e = (BossEnemy)m;
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

            // Phase time logic
            if (CheckCurrentPhaseTime(e))
                return;

            // Only make walk transition if player is not dead.
            if (player != null && currentPhaseTime > 0)
            {
                if (!CheckAttackRange(e))
                {
                    // Transition to walk
                    e.SetTransition(Transition.LostPlayerAttackRange);
                }
            }
            else
            {
                // Back to idle if all players are dead.
                e.SetTransition(Transition.AttackFinished);
                attackStarted = false;
                return;
            }
        }
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        base.DoBeforeEntering();

        Debug.Log("Boss: Ranged Attack State");

        if (currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();

        if (currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;


        //attackStarted = false;
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
            bool hit = Physics.Raycast(ray, out hitInfo, e.RangedAttackRange, 1 << playerLayer);

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
            Debug.DrawRay(enemyPos, (playerPos - enemyPos).normalized * e.RangedAttackRange, Color.yellow);

            return hit;
        }
        return false;
    }

    /// <summary>
    /// Timer logic for the phase time.
    /// Decreases the timer and makes the transition if it reaches its endpoint.
    /// </summary>
    private bool CheckCurrentPhaseTime(BossEnemy e)
    {
        // Increase current time.
        currentPhaseTime -= Time.deltaTime;

        // If the current phase time is finished, go do Idle.
        if (currentPhaseTime <= 0f)
        {
            e.SetTransition(Transition.AttackFinished);
            attackStarted = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attack logic of the boss.
    /// </summary>
    /// <param name="e">Boss reference</param>
    private void AttackPlayer(BossEnemy e)
    {
        // Attack only if allowed.
        if (attackAllowed && !attackStarted)
        {
            //Debug.Log("RangedBoss: Attack!");

            attackStarted = true;
            // Spawn bullet
            CreateBullet(e);
            attackAllowed = false;
        }


        // Timer logic.
        if (currentAttackTimer >= e.AttackInterval)
        {
            attackAllowed = true;
            currentAttackTimer = 0f;
        }

        // Increase attack timer.
        currentAttackTimer += Time.deltaTime;
    }

    /// <summary>
    /// Creates a bullet and spawns it.
    /// </summary>
    /// <param name="e">Reference to the boss enemy.</param>
    private void CreateBullet(BossEnemy e)
    {
        // Angle between two bullets
        float angleBetween = e.SpreadAngle / e.NumberOfBullets;

        // Start angle of the calculation
        float currentAngle = -(e.SpreadAngle / 2f);

        for (int i = 0; i < e.NumberOfBullets; i++)
        {
            GameObject g = GameObject.Instantiate(e.RangedBullet);
            BossBullet bullet;

            if (g != null && g.GetComponent<MonoBehaviour>() is BossBullet)
            {
                bullet = g.GetComponent<BossBullet>();
                bullet.OwnerScript = e;
                bullet.name = "BossBullet";
                bullet.Damage = bullet.Damage;

                bullet.transform.position = e.transform.position + new Vector3(0, 1, 0);
                float speed = bullet.BulletSpeed;
                Vector3 playerDirection = (e.TargetPlayer.transform.position - bullet.transform.position).normalized;
                bullet.transform.rotation = Quaternion.LookRotation(playerDirection);
                bullet.transform.Rotate(90, 0, 0);

                // Bullet rotation
                Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
                Vector3 v = playerDirection;
                Vector3 rotationVector = rotation * v;

                // Shoot
                bullet.Shoot(rotationVector, bullet.BulletSpeed);

                currentAngle += angleBetween;
            }
        }
    }
}
