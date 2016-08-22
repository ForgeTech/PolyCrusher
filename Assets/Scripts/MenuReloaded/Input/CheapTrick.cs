using UnityEngine;
using System.Collections;
using InControl;
public class CheapTrick : MonoBehaviour {


    public bool activate = false;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (activate)
        {
            activate = false;

            for(int i = 0; i < 10; i++)
            {

                //StartCoroutine(RumbleManager.Instance.ChargerSpecial(InputManager.ActiveDevice));
            }


        }


	}
    
}
