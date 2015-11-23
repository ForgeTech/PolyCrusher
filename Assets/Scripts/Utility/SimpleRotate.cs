using UnityEngine;
using System.Collections;

public class SimpleRotate : MonoBehaviour
{
    public Vector3 rotation = Vector3.zero;
	
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(rotation);
	}
}
