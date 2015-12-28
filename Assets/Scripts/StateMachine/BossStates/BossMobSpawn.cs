using UnityEngine;
using System.Collections;
using System;

public class BossMobSpawn : FSMState
{
    // Current phase timer.
    protected float currentPhaseTime = 0f;

    // The actual phase time.
    protected float phaseTime;

    // Current mob spawn timer.
    protected float currentMobSpawnTimer;

    public BossMobSpawn(float phaseTime, StateID id)
    {
        this.stateID = id;
        this.phaseTime = phaseTime;
        this.currentPhaseTime = this.phaseTime;

        this.currentMobSpawnTimer = 0f;
    }

    public override void Act(GameObject player, GameObject npc)
    {
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        if (m is BossEnemy)
        {
            BossEnemy e = (BossEnemy)m;

            SpawnMob(e);
        }
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        MonoBehaviour m = npc.GetComponent<MonoBehaviour>();

        if (m is BossEnemy)
        {
            BossEnemy e = (BossEnemy)m;

            CheckCurrentPhaseTime(e);
        }
    }

    /// <summary>
    /// Set entering conditions.
    /// </summary>
    public override void DoBeforeEntering()
    {
        Debug.Log("Boss: Mob Spawn State");

        if (currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;

        // Reset timer
        currentMobSpawnTimer = 0f;
    }

    /// <summary>
    /// Set leaving conditions.
    /// </summary>
    public override void DoBeforeLeaving()
    {
        base.DoBeforeLeaving();

        if (currentPhaseTime <= 0f)
            this.currentPhaseTime = this.phaseTime;

        // Reset timer
        currentMobSpawnTimer = 0f;
    }

    /// <summary>
    /// Timer logic for the phase time.
    /// Decreases the timer and makes the transition if it reaches its endpoint.
    /// </summary>
    protected virtual bool CheckCurrentPhaseTime(BossEnemy e)
    {
        // Increase current time.
        currentPhaseTime -= Time.deltaTime;

        // If the current phase time is finished, go do Idle.
        if (currentPhaseTime <= 0f)
        {
            e.SetTransition(Transition.AttackFinished);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns an enemy in the range of the boss.
    /// </summary>
    /// <param name="e">Boss enemy</param>
    protected virtual void SpawnMob(BossEnemy e)
    {
        if (currentMobSpawnTimer >= e.MobSpawnPhase.spawnInterval)
        {
            //Play animation.
            Animator anim = e.GetComponent<Animator>();

            if (anim != null)
                anim.SetTrigger("Special");

            Vector2 randomCirclePoint = UnityEngine.Random.insideUnitCircle * e.MobSpawnPhase.spawnRadius 
                + new Vector2(e.transform.position.x, e.transform.position.z);

            Vector3 spawnPosition = new Vector3(randomCirclePoint.x, e.transform.position.y, randomCirclePoint.y);

            NavMeshHit hit;
            bool posFound = NavMesh.SamplePosition(spawnPosition, out hit, 5f, NavMesh.AllAreas);

            if (posFound)
            {
                GameObject mob = GameObject.Instantiate(e.MobSpawnPhase.mobPrefab) as GameObject;
                mob.SetActive(false);
                mob.transform.position = hit.position;
                mob.SetActive(true);
            }

            // Reset timer
            currentMobSpawnTimer = 0f;
        }

        // Increase mob timer
        currentMobSpawnTimer += Time.deltaTime;
    }
}
