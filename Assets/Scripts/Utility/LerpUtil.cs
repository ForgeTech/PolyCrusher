using UnityEngine;
using System.Collections;

/// <summary>
/// Utility class for the Lerping of Position, Skaling, Rotation and Color.
/// The utility methods should be used as Coroutines.
/// </summary>
public static class LerpUtil
{
    /// <summary>
    /// Lerps from one to another position in a given time.
    /// This method does not affect the z-coordinate.
    /// </summary>
    /// <param name="obj">Object to move</param>
    /// <param name="newPosition">New position</param>
    /// <param name="time">Lerping time</param>
    /// <returns></returns>
    public static IEnumerator MoveToPoint(Transform obj, Vector3 newPosition, float time)
    {
        float elapsedTime = 0f;
        float t;
        Vector3 startPosition = obj.position;
        //newPosition.z = obj.position.z;

        while (elapsedTime < time && obj != null)
        {
            t = elapsedTime / time;

            obj.position = Vector3.Slerp(startPosition, newPosition, t);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (obj != null)
            obj.transform.position = newPosition;
    }

    /// <summary>
    /// Lerps from the original Scale to the new Scale in a given time.
    /// This method does not affect the z-coordinate.
    /// </summary>
    /// <param name="obj">Object to scale</param>
    /// <param name="newScale">New Scale</param>
    /// <param name="time">Lerping time</param>
    /// <returns></returns>
    public static IEnumerator ScaleLerp(Transform obj, Vector3 newScale, float time)
    {
        float elapsedTime = 0f;
        float t;
        Vector3 startScale = obj.localScale;
        //newScale.z = 1f;

        while (elapsedTime < time && obj != null)
        {
            t = elapsedTime / time;

            obj.localScale = Vector3.Lerp(startScale, newScale, t);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (obj != null)
            obj.localScale = newScale;
    }

    /// <summary>
    /// Lerps from the original color to the new Color in a specific time.
    /// </summary>
    /// <param name="obj">Object to lerp</param>
    /// <param name="newColor">New color</param>
    /// <param name="time">Lerping time</param>
    /// <returns></returns>
    public static IEnumerator ColorLerp(GameObject obj, Color newColor, float time)
    {
        float elapsedTime = 0f;
        float t;
        Material mat = obj.GetComponent<Renderer>().material;
        Color startColor = mat.color;
        

        //Emission
        //mat.SetColor("_EmissionColor", Color.black);

        while (elapsedTime < time && obj != null)
        {
            t = elapsedTime / time;

            obj.GetComponent<Renderer>().material.color = Color.Lerp(startColor, newColor, t);
            elapsedTime += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        if (obj != null)
            mat.color = newColor;
    }
}
