using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour {

	// serialized fields
	[Header("Tutorial border object")]
	[SerializeField]
	private GameObject tutorialBorder;                              // Canvas with sprite as border.

	[Space(5)]
	[Header("Tutorial object values")]
	[SerializeField]
	private Vector3 tutorialAddToPositionVector = new Vector3(0,0,6);
	[SerializeField]
	private GameObject tutorialExplosion;

	[Space(5)]
	[Header("Text button values")]
	[SerializeField]
	private GameObject tutorialButtonPrefab;						// ButtonPrefab of the tutorial buttons.
	[SerializeField]
	private GameObject tutorialButtonPositionObject;				// Gameobject which should determine the base position of the button objects.
	[SerializeField]
	private GameObject[] tutorialPrefabs;							// Prefabs of the tutorials.
	[SerializeField]
	private int ButtonFontSize = 50;								// Size of the Button.
	[SerializeField]
	private int ButtonAllignementDistance = 4;						// Distance between the several buttons.
	[SerializeField]
	private Vector3 textRotationVector = new Vector3(90,0,0);		// Rotationvector of the text button objects.
	[SerializeField]
	private const float CharacterSize = 0.5f;						// Size of the characters of the text.

	[Space(5)]
	[Header("Text button collider values")]
	private Vector3 colliderSize = new Vector3(2,4,2);				// Determines the defaultsize of the collider.

	[Space(5)]
	[Header("Debug values")]
	[SerializeField]
	private bool drawGizmos = true;									// Determines the defaultsize of the collider.

	// private fields
	private GameObject[] textButtons;								// Runtime generated text mesh buttons for tutorial prefab selection.
	private int borderTweenID = 0;									// Unique ID of the lean tween (needed for appropriate ending stopping method).
	private Vector3 borderStartSize;

	void Start() {
		LineBorderScript.TutorialLeft += ActivateTutorialTextButtons;
		textButtons = new GameObject[tutorialPrefabs.Length];
		tutorialBorder = Instantiate(tutorialBorder);
		tutorialBorder.GetComponentInChildren<Image>().enabled = false;

		borderStartSize = tutorialBorder.transform.localScale;

		// Create on-collision buttons for the tutorial prefab selection.
		for (int i = 0; i < tutorialPrefabs.Length; i++) {
			textButtons[i] = Instantiate(tutorialButtonPrefab);

			textButtons[i].transform.position = new Vector3(tutorialButtonPositionObject.transform.position.x + ButtonAllignementDistance * i
				, tutorialButtonPositionObject.transform.position.y
				, tutorialButtonPositionObject.transform.position.z);

			tutorialPrefabs[i].transform.position = new Vector3(textButtons[i].transform.position.x + tutorialAddToPositionVector.x
				, textButtons[i].transform.position.y + tutorialAddToPositionVector.y
				, textButtons[i].transform.position.z + tutorialAddToPositionVector.z);

			Text text = textButtons[i].GetComponentInChildren<Text>();
			text.text = (i + 1).ToString();

			textButtons[i].GetComponent<TextButtonScript>().Tutorial = tutorialPrefabs[i].GetComponent<BaseTutorial>() as ITutoriable;
		}
		TextButtonScript.TutorialActivated += DeactivateTextButtons;
	}

	void DeactivateTextButtons() {
		int i = 0;
		foreach(GameObject prefab in textButtons) {
			if (!prefab.GetComponent<TextButtonScript>().IsActive) {
				prefab.GetComponentInChildren<Text>().color = Color.red;
			} else {
				CameraManager.CameraReference.ShakeOnce();						// Maybe more fancy explosions?
				TutorialExplosion(tutorialPrefabs[i].transform.position);

				tutorialBorder.transform.position = tutorialPrefabs[i].transform.position;
				tutorialBorder.GetComponentInChildren<Image>().enabled = true;
				tutorialBorder.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);

				LeanTween.value(tutorialBorder.transform.GetChild(0).gameObject, tutorialBorder.GetComponentInChildren<Image>().color.g, 0, 0.5f).setOnUpdate((float val) => {
					tutorialBorder.GetComponentInChildren<Image>().color = new Color(val, 1f, val, 1 + (-1*val));
				});
				LeanTween.value(tutorialBorder.transform.GetChild(0).gameObject, tutorialBorder.GetComponentInChildren<Image>().color.g, 0, 0.5f).setOnUpdate((float val) => {
					tutorialBorder.GetComponentInChildren<Image>().color = new Color(val, 1f, val, 1 + (-1 * val));
				});

				tutorialBorder.transform.localScale = borderStartSize;
				borderTweenID = LeanTween.scale(tutorialBorder, new Vector3(1.3f, 1.1f, 1f), 1.5f).setLoopPingPong().setEase(LeanTweenType.easeInOutSine).id;
			}
			i++;
		}
	}

	void ActivateTutorialTextButtons() {
		LeanTween.cancel(borderTweenID);

		foreach (GameObject prefab in textButtons) {
			if (!prefab.GetComponent<TextButtonScript>().IsActive) {
				prefab.GetComponentInChildren<Text>().color = Color.yellow;
			} else {
				LeanTween.value(tutorialBorder.transform.GetChild(0).gameObject, tutorialBorder.GetComponentInChildren<Image>().color.a, 0, 0.5f).setOnUpdate((float val) => {
					tutorialBorder.GetComponentInChildren<Image>().color = new Color(tutorialBorder.GetComponentInChildren<Image>().color.r
						, tutorialBorder.GetComponentInChildren<Image>().color.g
						, tutorialBorder.GetComponentInChildren<Image>().color.b
						, val);
				});
				//tutorialBorder.GetComponentInChildren<Image>().enabled = false;
				prefab.GetComponent<TextButtonScript>().IsActive = false;
			}
		}
	}

	void TutorialExplosion(Vector3 position) {
		if (tutorialExplosion != null) {
			GameObject particle = Instantiate(tutorialExplosion) as GameObject;
			particle.transform.position = position;
		} else {
			Debug.LogWarning("TutorialManager: tutorialExplosion gameobject not found!");
		}
	}

	void OnDrawGizmos() {
		if (!drawGizmos) {
			return;				// Escape silently!
		}

		for (int i = 0; i < tutorialPrefabs.Length; i++) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(new Vector3(tutorialButtonPositionObject.transform.position.x + ButtonAllignementDistance * i, tutorialButtonPositionObject.transform.position.y, tutorialButtonPositionObject.transform.position.z), 0.5f);
		}
	}
}
