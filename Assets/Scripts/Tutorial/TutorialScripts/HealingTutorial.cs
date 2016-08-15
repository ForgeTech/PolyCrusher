using UnityEngine;

public class HealingTutorial : BaseTutorial {

	public override void StartTutorial() {
		EnableProjector();
		foreach (GameObject player in players) {
			player.GetComponent<BasePlayer>().Health = 1;
		}
	}

	public override void StopTutorial() {
		DisableProjector();
		foreach (GameObject player in players) {
			player.GetComponent<BasePlayer>().Health = player.GetComponent<BasePlayer>().MaxHealth;
		}
	}
}
