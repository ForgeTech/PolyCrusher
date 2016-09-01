using UnityEngine;
using System.Collections;

public class BasicWeaponTutorial : BaseTutorial {

	[SerializeField]
	private GameObject enemyPrefab;
	[SerializeField]
	private uint enemySpawnCount = 1;       // Determines how many enemies are getting spawned.
	private bool repeat = false;

	public override void StartTutorial() {
		repeat = true;
		EnableProjector();
		SpawnEnemies(enemySpawnCount);
		SetNotToMaxEnergy();

		InvokeRepeating("CheckForEnemies", 1, 2);
		InvokeRepeating("SetNotToMaxEnergy", 1, 2);
	}

	public override void StopTutorial() {
		repeat = false;

		CancelInvoke("CheckForEnemies");
		CancelInvoke("SetNotToMaxEnergy");

		DisableProjector();

		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
			BaseEnemy e = enemy.GetComponent<MonoBehaviour>() as BaseEnemy;

			LeanTween.scale(enemy, new Vector3(0.1f, 0.1f, 0.1f), 0.5f).setEase(LeanTweenType.easeOutBounce);
			StartCoroutine(DestroyAfterTime(enemy, 0.55f));
		}
	}

	IEnumerator DestroyAfterTime(GameObject go, float time) {
		yield return new WaitForSeconds(time);
		if (go != null) {
			go.GetComponent<NavMeshAgent>().enabled = true;
			Destroy(go);
		}
	}

	void SpawnEnemies(uint enemýCount) {
		for (int i = 0; i < enemýCount; i++) {
			GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3((0.1f + i / 2), 0, 0), enemyPrefab.transform.rotation) as GameObject;

			BaseEnemy e = enemy.GetComponent<MonoBehaviour>() as BaseEnemy;
			e.MovementSpeed = 0;
			e.MeleeAttackDamage = 0;
			e.MaxHealth = 100;
			e.Health = 100;

			enemy.GetComponent<NavMeshAgent>().avoidancePriority = 0;
			enemy.GetComponent<NavMeshAgent>().speed = 0;

			enemy.GetComponent<Rigidbody>().mass = 100;
		}
	}

	void CheckForEnemies() {
		if (repeat && GameObject.FindGameObjectWithTag("Enemy") == null) {
			SpawnEnemies(enemySpawnCount);
		}
	}

	void SetNotToMaxEnergy() {
		if (repeat) {
			foreach (GameObject player in players) {
				player.GetComponent<BasePlayer>().Energy = player.GetComponent<BasePlayer>().MaxEnergy - player.GetComponent<BasePlayer>().MaxEnergy / 10;
			}
		}
	}
}
