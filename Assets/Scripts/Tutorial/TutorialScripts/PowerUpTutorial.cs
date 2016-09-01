using UnityEngine;
using System.Collections;

public class PowerUpTutorial : BaseTutorial {

	[SerializeField]
	private GameObject pissingPete_1;				// Pissing Pete prefab 1.
	[SerializeField]
	private GameObject pissingPete_2;				// Pissing Pete prefab 2.
	[SerializeField]
	private Vector3 pissingPeteDistanceDifference = new Vector3(5, 0, 0);

	void Awake() {
		pissingPete_1.SetActive(false);
		pissingPete_2.SetActive(false);
	}

	public override void StartTutorial() {
		EnableProjector();

		pissingPete_1.transform.position = transform.position - pissingPeteDistanceDifference;
		pissingPete_2.transform.position = transform.position + pissingPeteDistanceDifference;
		pissingPete_1.SetActive(true);
		pissingPete_2.SetActive(true);


	}

	public override void StopTutorial() {
		DisableProjector();
		foreach (GameObject player in players) {
			player.GetComponent<BasePlayer>().Health = player.GetComponent<BasePlayer>().MaxHealth;
		}
	}
}
