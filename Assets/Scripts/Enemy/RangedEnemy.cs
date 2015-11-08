using UnityEngine;
using System.Collections;


/// <summary>
/// Ranged enemy behaviour.
/// The enemy shoots when it is in the attack range.
/// </summary>
public class RangedEnemy : BaseEnemy 
{
    // Prefab for the enemy bullet which will be spawned.
    [Space(5)]
    [Header("Bullet prefab")]
    [SerializeField]
    protected GameObject bulletPrefab;

    protected override void MakeFSM()
    {
        // Follow behaviour
        FollowPlayer follow = new FollowPlayer(attackRange, playerAttackLayer);
        follow.AddTransition(Transition.InPlayerAttackRange, StateID.AttackPlayer);
        follow.AddTransition(Transition.ReachedDestination, StateID.Idle);

        // Attack behaviour
        ShootPlayer shoot = new ShootPlayer(attackRange, playerAttackLayer, attackInterval);
        shoot.AddTransition(Transition.LostPlayerAttackRange, StateID.FollowPlayer);
        shoot.AddTransition(Transition.ReachedDestination, StateID.Idle);

        // Idle behaviour
        IdleEnemy idle = new IdleEnemy();
        idle.AddTransition(Transition.SawPlayer, StateID.FollowPlayer);

        fsm = new FSMSystem();
        fsm.AddState(follow);
        fsm.AddState(shoot);
        fsm.AddState(idle);
    }

    public override void Attack()
    {
        if (targetPlayer.GetComponent<MonoBehaviour>() is IDamageable)
        {
            //GameObject g = (GameObject) Instantiate(bulletPrefab);
            GameObject g = ObjectsPool.Spawn(bulletPrefab, Vector3.zero, bulletPrefab.transform.rotation);
            Bullet b = g.GetComponent<MonoBehaviour>() as Bullet;

            b.OwnerScript = this;
            g.name = "RangedBullet";
            g.transform.position = new Vector3(transform.position.x, 0.6f, transform.position.z);
            g.transform.rotation = Quaternion.LookRotation(transform.forward);

            //Debug.Log("Attack - " + g.name + " " + g.transform.position);

            b.Damage = MeleeAttackDamage;
            b.Shoot(transform.forward, b.BulletSpeed);

            if (anim != null)
                anim.SetTrigger("Attack");
        }
    }
}
