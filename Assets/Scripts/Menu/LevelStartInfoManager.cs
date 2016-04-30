using UnityEngine;
using System.Collections;

public class LevelStartInfoManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (GameObject.FindObjectOfType<LevelStartInformation>() == null)
        {
            // TODO: Remove Resource.Load to move the Manager folder out of the Resources folder!!!
            Instantiate(Resources.Load<GameObject>("Manager/LevelStartInformationContainer"));
        }
    }
}
