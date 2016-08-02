using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class DefaultColorTransition : TransitionHandlerInterface
{
    // In ms
    private const float LERP_TIME = 0.2f;

    private readonly Dictionary<GameObject, Coroutine> coroutineMap = new Dictionary<GameObject, Coroutine>();

    public void OnDefocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();

        if (info != null)
            SetButtonColor(gameobject, info.NormalColor);
        else
            Debug.LogError("NavigationInformation component is null!");
    }

    public void OnFocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();

        if (info != null)
            SetButtonColor(gameobject, info.HighlightedColor);
        else
            Debug.LogError("NavigationInformation component is null!");
    }

    private void SetButtonColor(GameObject gameobject, Color buttonColor)
    {
        Image image = gameobject.GetComponent<Image>();
        if (image != null)
        {
            ResetCoroutine(gameobject);
            Color startColor = new Color(image.color.r, image.color.g, image.color.b);
            coroutineMap.Add(gameobject, gameobject.GetComponent<MonoBehaviour>().StartCoroutine(FadeColor(image, startColor, buttonColor)));
        }
        else
            Debug.LogError("Image component is null!");
    }

    private void ResetCoroutine(GameObject gameobject)
    {
        if (coroutineMap.ContainsKey(gameobject))
        {
            Coroutine c;
            coroutineMap.TryGetValue(gameobject, out c);
            gameobject.GetComponent<MonoBehaviour>().StopCoroutine(c);
            coroutineMap.Remove(gameobject);
        }
    }

    IEnumerator FadeColor(Image image, Color start, Color end)
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / LERP_TIME)
        {
            image.color = Color.Lerp(start, end, t);
            yield return null;
        }
        image.color = end;
    }
}