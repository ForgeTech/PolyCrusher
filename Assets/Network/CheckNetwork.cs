using UnityEngine;
using System.Collections;

public class CheckNetwork : MonoBehaviour {
	
	// Use this for initialization
	void Awake () {

		if (GameObject.FindObjectOfType<PlayerNetCommunicate> () == null) {
			Instantiate(Resources.Load<GameObject>("Manager/_Network"));
		}
	
	}
}
