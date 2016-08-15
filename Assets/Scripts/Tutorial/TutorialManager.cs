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


	[Space(5)]
	[Header("Text button values")]
	[SerializeField]
	private GameObject tutorialButtonPrefab;						// ButtonPrefab of the tutorial buttons.
	[SerializeField]
	private GameObject tutorialButtonPositionObject;				// Gameobject which should determine the base position of the button objects.
	[SerializeField]
	private GameObject[] tutorialPrefabs;							// Prefabs of the tutorials.
	[SerializeField]
	private const int ButtonFontSize = 50;							// Size of the Button.
	[SerializeField]
	private const int ButtonAllignementDistance = 4;				// Distance between the several buttons.
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


	void Start() {
		LineBorderScript.TutorialLeft += ActivateTutorialTextButtons;
		textButtons = new GameObject[tutorialPrefabs.Length];
		tutorialBorder = Instantiate(tutorialBorder);
		tutorialBorder.GetComponentInChildren<Image>().enabled = false;

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
				prefab.GetComponentInChildren<Text>().color = Color.grey;
			} else {
				tutorialBorder.transform.position = prefab.transform.position;
				tutorialBorder.GetComponentInChildren<Image>().enabled = true;
			}
			i++;
		}
	}

	void ActivateTutorialTextButtons() {
		foreach (GameObject prefab in textButtons) {
			if (!prefab.GetComponent<TextButtonScript>().IsActive) {
				prefab.GetComponentInChildren<Text>().color = Color.yellow;
			} else {
				tutorialBorder.GetComponentInChildren<Image>().enabled = false;
				prefab.GetComponent<TextButtonScript>().IsActive = false;
			}
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
