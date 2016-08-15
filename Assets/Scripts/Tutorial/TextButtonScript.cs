using UnityEngine;
using System.Collections;

/// <summary>
/// Event handler for tutorial activation.
/// </summary>
public delegate void TutorialActivatedHandler();

public class TextButtonScript : MonoBehaviour {

	// properties
	public bool IsActive {
		get { return isActive; }
		set { isActive = value; }
	}
	public ITutoriable Tutorial {
		set { tutorial = value; }
	}

	// private fields
	private ITutoriable tutorial;
	private bool isActive = false;
	private bool processActivation = true;

	// Event handler for tutorial activation.
	public static event TutorialActivatedHandler TutorialActivated;

	void Start() {
		LineBorderScript.TutorialLeft += SetProcessActivationTrue;
		TutorialActivated += DeactivateTextButton;
	}

	/// <summary>
	/// Handles the trigger of the textbutton if a player enters the collider.
	/// </summary>
	/// <param name="collider">Colliding object.</param>
	void OnTriggerEnter(Collider collider) {
		if (isActive || !processActivation) {
			return;					// Escape silently!
		}
		
		if (collider.CompareTag("Player")) {
			TutorialActivated -= DeactivateTextButton;
			isActive = true;
			TutorialActivated();
			TutorialActivated += DeactivateTextButton;

			if (tutorial != null) {
				tutorial.StartTutorial();
			} else {
				Debug.LogWarning("No Tutorial is set in textButtonScript object!");
			}
		}
	}

	void DeactivateTextButton() {
		processActivation = false;
		isActive = false;
	}

	void SetProcessActivationTrue() {
		processActivation = true;
	}
}
