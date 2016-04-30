using UnityEngine;
using System.Collections;

/// <summary>
/// Uses the area of damage object and deals damage when it reaches it's max size.
/// All players in the circle get a damage and will be pushed outside.
/// </summary>
public class BossMeleeScript : AttackVisualizationScript
{
    // Damage of the attack.
    public int damage;

    // Specifies if the script has been initialized.
    [HideInInspector]
    public bool attackStarted;

    // Layer of the players
    private int playerLayer = 8;

    // Owner
    BossEnemy owner;

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
	}

    protected override void Update()
    {
        if (this.attackStarted)
        {
            base.Update();

            // Material Lerp
            rend.material.Lerp(startColor, endColor, currentTime / lerpTime);
        }
    }

    protected override void FadeOutHandler()
    {
        DealDamage();
        currentTime = 0;
        FadeOutAnimation();
        Destroy(this.gameObject, 0.28f);
    }

    /// <summary>
    /// Initializes the behaviour.
    /// </summary>
    public void InitMeleeScript(float damageRadius, float activationTime, BossEnemy owner, int damage)
    {
        this.attackStarted = true;
        this.effectRadius = damageRadius;
        this.lerpTime = activationTime;
        this.owner = owner;
        this.damage = damage;

        FadeInAnimation();
    }

    /// <summary>
    /// Initializes the behaviour with the standard values defined in the prefab.
    /// </summary>
    public void InitMeleeScript(BossEnemy owner)
    {
        InitMeleeScript(this.effectRadius, this.lerpTime, owner, this.damage);
    }

    /// <summary>
    /// Deals damage to all players inside the radius.
    /// </summary>
    private void DealDamage()
    {
        Transform[] players = GetAllPlayersInRadius(effectRadius);

        for (int i = 0; i < players.Length; i++)
        {
            MonoBehaviour m = players[i].GetComponent<MonoBehaviour>();

            if (m is BasePlayer)
            {
                m.GetComponent<BasePlayer>().TakeDamage(damage, owner);
                m.GetComponent<Rigidbody>().AddForce((players[i].position - (transform.position + new Vector3(0.05f, 0, 0))).normalized * owner.PushAwayForce, ForceMode.Impulse);
            }
        }

        if (sound != null)
            SoundManager.SoundManagerInstance.Play(sound, transform.position, soundVolume, soundPitch);

        if (explosionParticle != null)
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
    }

    private Transform[] GetAllPlayersInRadius(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, 1 << playerLayer);
        Transform[] players = new Transform[hits.Length];

        for (int i = 0; i < hits.Length; i++)
            players[i] = hits[i].transform;

        return players;
    }
}
