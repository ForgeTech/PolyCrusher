using UnityEngine;
using System.Collections;

public class PolyTutorial : BaseTutorial {

	[SerializeField]
	private GameObject enemyPrefab;
	[SerializeField]
	private uint enemySpawnCount = 1;		// Determines how many enemies are getting spawned.
	private bool checkForEnemies = false;

	public override void StartTutorial() {
		checkForEnemies = true;
		EnableProjector();
		SpawnEnemies(enemySpawnCount);
		SetToMaxEnergy();

		InvokeRepeating("CheckForEnemies", 2, 2);
		InvokeRepeating("SetToMaxEnergy", 2, 2);
	}

	public override void StopTutorial() {
		checkForEnemies = false;

		CancelInvoke("CheckForEnemies");
		CancelInvoke("SetToMaxEnergy");

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
			enemy.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

			enemy.GetComponent<NavMeshAgent>().avoidancePriority = 0;
			enemy.GetComponent<NavMeshAgent>().enabled = false;

			enemy.GetComponent<Rigidbody>().mass = 100;

			BaseEnemy e = enemy.GetComponent<MonoBehaviour>() as BaseEnemy;
			e.enabled = false;
			e.MeleeAttackDamage = 0;
			e.MaxHealth = 99999;
			e.Health = 99999;

			LeanTween.scale(enemy, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutBounce);
			enemy.GetComponent<NavMeshAgent>().enabled = true;
		}
	}

	void CheckForEnemies() {
		if (checkForEnemies && GameObject.FindGameObjectWithTag("Enemy") == null) {
			SpawnEnemies(enemySpawnCount);
		}
	}

	void SetToMaxEnergy() {
		foreach (GameObject player in players) {
			player.GetComponent<BasePlayer>().Energy = player.GetComponent<BasePlayer>().MaxEnergy;
		}
	}
}
