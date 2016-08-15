using UnityEngine;

public class BaseTutorial : MonoBehaviour, ITutoriable {

	// private fields
	protected LineRenderer lineRenderer;		// Line renderer field.
	protected Projector projector;				// Projector component field.
	protected GameObject[] players;				// Array of the player gameobjects.

	void Start() {
		players = GameObject.FindGameObjectsWithTag("Player");
		projector = gameObject.GetComponent<Projector>();
		projector.enabled = false;
	}

	protected virtual void EnableProjector() { projector.enabled = true; }

	protected virtual void DisableProjector() {	projector.enabled = false; }

	public virtual void StartTutorial() { Debug.LogWarning("Tutorial without corresponding start function detected!"); }

	public virtual void StopTutorial() { Debug.LogWarning("Tutorial without corresponding stop function detected!"); }
}
