using UnityEngine;
using System.Collections;

/// <summary>
/// Uses the area of damage object and deals damage when it reaches it's max size.
/// All players in the circle get a damage and will be pushed outside.
/// </summary>
public class BossMeleeScript : MonoBehaviour
{
    // Radius of the damage area.
    public float damageRadius;

    // Damage of the attack.
    public int damage;

    // Time until damage will taken.
    public float activationTime;

    // Current timer value.
    private float currentTime;

    // Specifies if the script has been initialized.
    [HideInInspector]
    public bool attackStarted;

    // Layer of the players
    private int playerLayer = 8;

    // Owner
    BossEnemy owner;

    // The height of the tween animation.
    public float easeInHeight = 4f;

    // Use this for initialization
    void Start ()
    {
        transform.localScale = Vector3.zero;
        //this.attackStarted = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (this.attackStarted)
        {
            // TODO: attack started cant be set to true
            if (currentTime >= activationTime)
            {
                DealDamage();
                currentTime = 0;
                StartCoroutine(transform.MoveTo(transform.position + new Vector3(0, easeInHeight, 0), 0.2f, Ease.CubeIn));
                StartCoroutine(transform.ScaleTo(Vector3.zero, 0.2f, Ease.CubeIn));
                Destroy(this.gameObject, 0.28f);
            }

            currentTime += Time.deltaTime;
        }
	}

    /// <summary>
    /// Initializes the behaviour.
    /// </summary>
    public void InitMeleeScript(float damageRadius, float activationTime, BossEnemy owner, int damage)
    {
        this.attackStarted = true;
        this.damageRadius = damageRadius;
        this.activationTime = activationTime;
        this.owner = owner;
        this.damage = damage;

        Vector3 originalPos = transform.position;
        transform.position += new Vector3(0, easeInHeight, 0);

        StartCoroutine(transform.MoveTo(originalPos, activationTime * 0.5f, Ease.CubeOut));
        StartCoroutine(transform.ScaleTo(new Vector3(damageRadius, damageRadius, 0.3f), activationTime, Ease.CubeOut));
    }

    /// <summary>
    /// Initializes the behaviour with the standard values defined in the prefab.
    /// </summary>
    public void InitMeleeScript(BossEnemy owner)
    {
        InitMeleeScript(this.damageRadius, this.activationTime, owner, this.damage);
    }

    /// <summary>
    /// Deals damage to all players inside the radius.
    /// </summary>
    private void DealDamage()
    {
        Transform[] players = GetAllPlayersInRadius(damageRadius);

        for (int i = 0; i < players.Length; i++)
        {
            MonoBehaviour m = players[i].GetComponent<MonoBehaviour>();

            if (m is BasePlayer)
            {
                m.GetComponent<BasePlayer>().TakeDamage(damage, owner);
                m.GetComponent<Rigidbody>().AddExplosionForce(owner.PushAwayForce, transform.position, damageRadius);
            }
        }

        // TODO: Play sound
        // TODO: Spawn Particle
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
