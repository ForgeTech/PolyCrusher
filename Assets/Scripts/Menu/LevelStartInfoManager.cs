using UnityEngine;
using System.Collections;

public class LevelStartInfoManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (GameObject.FindObjectOfType<LevelStartInformation>() == null)
        {
            Instantiate(Resources.Load<GameObject>("Manager/LevelStartInformationContainer"));
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
