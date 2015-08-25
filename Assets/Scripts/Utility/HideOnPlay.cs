using UnityEngine;
using System.Collections;

/// <summary>
/// Disables the mesh renderer on the game start.
/// </summary>
public class HideOnPlay : MonoBehaviour 
{
    /// <summary>
    /// Disable mesh renderer on start.
    /// </summary>
	void Start ()
    {
        if (this.gameObject.GetComponent<MeshRenderer>() != null)
            gameObject.GetComponent<MeshRenderer>().enabled = false;
	}
}
