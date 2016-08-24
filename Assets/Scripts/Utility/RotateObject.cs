using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates the gameobject with the sinus and cosinus function.
/// </summary>

//[ExecuteInEditMode]
public class RotateObject : MonoBehaviour 
{
    public float rotationSpeedX = 3f;
    public float rotationSpeedY = 6f;
    public float rotationSpeedZ = 4f;
	
	// Update is called once per frame
	void Update () 
    {
        gameObject.transform.Rotate(new Vector3(Mathf.Sin(Time.deltaTime) * rotationSpeedX,
            Mathf.Cos(Time.deltaTime) * rotationSpeedY,
            -Mathf.Sin(Time.deltaTime) * rotationSpeedZ)*Time.timeScale);
	}
}