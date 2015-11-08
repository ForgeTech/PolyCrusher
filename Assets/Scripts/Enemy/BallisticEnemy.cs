using UnityEngine;
using System.Collections;

public class BallisticEnemy : RangedEnemy
{
	[SerializeField]
	private float minimumRange = 3f;

	public override void Attack ()
	{
		if (targetPlayer.GetComponent<MonoBehaviour>() is IDamageable)
		{
            //GameObject g = (GameObject) Instantiate(bulletPrefab);
            GameObject g = ObjectsPool.Spawn(bulletPrefab, Vector3.zero, bulletPrefab.transform.rotation);
            Rocket r = g.GetComponent<MonoBehaviour>() as Rocket;
            r.OwnerScript = this;
			r.gameObject.transform.localScale = new Vector3(1f, 1f,1f);
			
			g.name = "RangedRocket";
			g.transform.position = new Vector3(transform.position.x, transform.position.y + 0.8f, transform.position.z);
			g.transform.rotation = Quaternion.LookRotation(transform.forward);
			
			//Debug.Log("Attack - " + g.name + " " + g.transform.position);
			
			r.Damage = MeleeAttackDamage;
			if (Mathf.Abs(targetPlayer.position.x - transform.position.x) <= minimumRange && Mathf.Abs(targetPlayer.position.z - transform.position.z) <= minimumRange) {
				Vector3 direction = (gameObject.transform.forward * minimumRange);
				Vector3 targetPos = gameObject.transform.position + direction;
				//r.Shoot (new Vector3(targetPos.x, -0.1f, targetPos.z));
				r.Shoot (targetPos);
			} else {
				//r.Shoot( new Vector3(targetPlayer.position.x, - 0.1f,targetPlayer.position.z));
				r.Shoot (targetPlayer.position);
			}

			if (anim != null)
				anim.SetTrigger("Attack");
		}
	}

}
