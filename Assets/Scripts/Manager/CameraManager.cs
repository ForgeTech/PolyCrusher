using UnityEngine;
using System.Collections;

/// <summary>
/// Stores a reference to the main camera and allows the access.
/// </summary>
public class CameraManager : MonoBehaviour
{
    /// <summary>
    /// Returns the main camera.
    /// </summary>
    public static CameraSystem CameraReference
    {
        get
        {
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");

            if (cam != null)
                return cam.transform.parent.GetComponent<CameraSystem>();
            else
                return null;
        }
    }
}
