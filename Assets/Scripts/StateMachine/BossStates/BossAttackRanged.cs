using UnityEngine;
using System.Collections;
using System;

public class BossAttackRanged : BossAttackMelee
{
    public BossAttackRanged(float phaseTime, StateID id) : base(phaseTime, id)
    { }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        Debug.Log("Boss: Ranged Attack State");

        if (currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;
    }

    /// <summary>
    /// Checks the range to the player and returns true if the player is in range and false
    /// if the player is out of range.
    /// </summary>
    /// <param name="e">Reference to the boss enemy.</param>
    /// <returns>True: Player is in range, False: Player is not in range.</returns>
    protected override bool CheckAttackRange(BossEnemy e)
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
                    if (e.TargetPlayer != null && e.TargetPlayer.GetComponent<BasePlayer>() != null)
                    {
                        // If the names aren't equal there is no hit.
                        if (((BasePlayer)m).PlayerName != e.TargetPlayer.GetComponent<BasePlayer>().PlayerName)
                            hit = false;
                    }
                }
            }

            // Debug draw Ray
            Debug.DrawRay(enemyPos, (playerPos - enemyPos).normalized * e.RangedAttackRange, Color.yellow);

            return hit;
        }
        return false;
    }


    /// <summary>
    /// Attack logic of the boss.
    /// </summary>
    /// <param name="e">Boss reference</param>
    protected override void AttackPlayer(BossEnemy e)
    {
        // Attack only if allowed.
        if (attackAllowed && !attackStarted)
        {
            Debug.Log("RangedBoss: Attack!");

            //Play animation.
            Animator anim = e.GetComponent<Animator>();

            if (anim != null)
                anim.SetTrigger("Shoot");

            attackStarted = true;
            // Spawn bullet
            CreateBullet(e);
            attackAllowed = false;
        }


        // Timer logic.
        if (currentAttackTimer >= e.RangedAttackInterval)
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
            //GameObject g = GameObject.Instantiate(e.RangedBullet);
            GameObject g = ObjectsPool.Spawn(e.RangedBullet, Vector3.zero, e.RangedBullet.transform.rotation);
            BossBullet bullet;

            if (g != null && g.GetComponent<MonoBehaviour>() is BossBullet)
            {
                bullet = g.GetComponent<BossBullet>();
                bullet.OwnerScript = e;
                bullet.name = "BossBullet";
                bullet.Damage = bullet.Damage;

                bullet.transform.position = e.transform.position + new Vector3(0, 1, 0);
                Vector3 playerDirection = (e.TargetPlayer.position - bullet.transform.position).normalized;
                // Eliminate y
                playerDirection = new Vector3(playerDirection.x, 0f, playerDirection.z);
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
