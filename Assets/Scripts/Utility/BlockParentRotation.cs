using UnityEngine;
using System.Collections;


/// <summary>
/// When this script is attached to a Gameobject, it will block the rotation of the parent object to the given value.
/// </summary>
public class BlockParentRotation : MonoBehaviour 
{
    [Tooltip("The rotation of the gameobject which should be locked.")]
    [SerializeField]
    private Vector3 lockRotation = Vector3.zero;


	void LateUpdate () 
    {
	    // Block the rotation
        transform.rotation = Quaternion.Euler(lockRotation);
	}
}
