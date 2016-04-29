using UnityEngine;
using System.Collections;

public class CheckNetwork : MonoBehaviour {
    [SerializeField]
    private GameObject networkManagerPrefab;

	// Use this for initialization
	void Awake () {
		if (GameObject.FindObjectOfType<PlayerNetCommunicate> () == null) {
            if (networkManagerPrefab != null)
                Instantiate(networkManagerPrefab);
            else
                throw new System.Exception("Network Manager prefab is null!");
		}
	}
}