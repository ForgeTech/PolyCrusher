using UnityEngine;
using System.Collections;

/// <summary>
/// Event handler for tutorial deactivation.
/// </summary>
public delegate void TutorialLeftHandler();

public class LineBorderScript : MonoBehaviour {

	// Event handler if a player leaved the tutorial collider.
	public static event TutorialActivatedHandler TutorialLeft;

	void OnTriggerExit(Collider collider) {
		if (collider.CompareTag("Player")) {
			TutorialLeft();
		}
	}

}
