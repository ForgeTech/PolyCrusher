using UnityEngine;
using System.Collections;

public class PowerUpTutorial : BaseTutorial {

	[SerializeField]
	private Vector3 pissingPeteDistanceDifference = new Vector3(5, 0, 0);
	[SerializeField]
	private GameObject pissingPete_1;				// Pissing Pete prefab 1.
	[SerializeField]
	private GameObject pissingPete_2;				// Pissing Pete prefab 2.
	private GameObject collectibleManager;

	void Awake() {
		collectibleManager = GameObject.Find("_CollectibleManager");
		collectibleManager.SetActive(false);
	}

	void Start() {
		pissingPete_1.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		pissingPete_2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

		players = GameObject.FindGameObjectsWithTag("Player");
		projector = gameObject.GetComponent<Projector>();
		projector.enabled = false;
		LineBorderScript.TutorialLeft += StopTutorial;

		pissingPete_1.SetActive(false);
		pissingPete_2.SetActive(false);
	}

	public override void StartTutorial() {
		EnableProjector();

		pissingPete_1.transform.position = transform.position - pissingPeteDistanceDifference;
		pissingPete_2.transform.position = transform.position + pissingPeteDistanceDifference;
		pissingPete_1.SetActive(true);
		pissingPete_2.SetActive(true);

		LeanTween.scale(pissingPete_1, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.scale(pissingPete_2, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutBounce);

		collectibleManager.SetActive(true);
		collectibleManager.GetComponent<CollectibleManager>().enabled = true;
	}

	public override void StopTutorial() {
		DisableProjector();
		PowerUpItem.PowerUpCollected -= collectibleManager.GetComponent<CollectibleManager>().checkSpawn;

		LeanTween.scale(pissingPete_1, new Vector3(0.05f, 0.05f, 0.05f), 0.5f).setEase(LeanTweenType.easeOutBounce);
		LeanTween.scale(pissingPete_2, new Vector3(0.05f, 0.05f, 0.05f), 0.5f).setEase(LeanTweenType.easeOutBounce);

		StartCoroutine(DisableAfterTime(pissingPete_1, 0.525f));
		StartCoroutine(DisableAfterTime(pissingPete_2, 0.525f));

		collectibleManager.SetActive(false);
		collectibleManager.GetComponent<CollectibleManager>().enabled = false;
	}

	IEnumerator DisableAfterTime(GameObject go, float time) {
		yield return new WaitForSeconds(time);
		if (go != null) {
			go.SetActive(false);
		}
		foreach (PowerUpItem powerUp in GameObject.FindObjectsOfType<PowerUpItem>()) {
			Destroy(powerUp.gameObject);
		}
	}
}
