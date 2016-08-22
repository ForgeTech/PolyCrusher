using UnityEngine;
using System.Collections;


/// <summary>
/// Coroutine helper class to call coroutines from every script.
/// Is implemented as singleton.
/// </summary>
public class CoroutineHelper : MonoBehaviour 
{

    private static CoroutineHelper couroutineHelperInstance;

    public static CoroutineHelper CouroutineHelperInstance
    {
        get 
        {
            //If the instance isn't set yet, it will be set (Happens only the first time!)
            if (couroutineHelperInstance == null)
            {
                couroutineHelperInstance = GameObject.FindObjectOfType<CoroutineHelper>();
                DontDestroyOnLoad(couroutineHelperInstance);
            }

            return couroutineHelperInstance;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
