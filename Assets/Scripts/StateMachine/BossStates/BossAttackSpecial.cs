using UnityEngine;
using System.Collections;

public class BossAttackSpecial : BossAttackMelee
{
    public BossAttackSpecial(float phaseTime, StateID id) : base(phaseTime, id)
    {
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        Debug.Log("Boss: Special Attack State");

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
                    if (e.TargetPlayer != null)
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
            //Play animation.
            Animator anim = e.GetComponent<Animator>();

            if (anim != null)
                anim.SetTrigger("Special");

            attackStarted = true;

            // Spawn meteorits
            CreateMeteorField(e);
            
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
    /// Spawns the meteor field.
    /// </summary>
    private void CreateMeteorField(BossEnemy e)
    {
        GameObject g = GameObject.Instantiate(e.MeteorAttackPrefab);

        if (g.GetComponent<BossMeteorScript>())
        {
            g.transform.position = e.TargetPlayer.position + new Vector3(0, e.MeteorSpawnHeight, 0);
            g.GetComponent<BossMeteorScript>().InitializeScript(e);
        }
    }
}
