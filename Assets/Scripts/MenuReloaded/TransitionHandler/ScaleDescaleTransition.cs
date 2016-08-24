using UnityEngine;
using System;

/// <summary>
/// Scale and Descale transition based on the NavigationInformation values.
/// </summary>
public class ScaleDescaleTransition : TransitionHandlerInterface
{
    // In ms
    private const float LERP_TIME = 0.2f;

    public void OnDefocus(GameObject gameobject)
    {
        RectTransform rect = gameobject.GetComponent<RectTransform>();
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        LeanTween.scale(rect, info.DeselectedScale, LERP_TIME).setEase(info.EaseType).setUseEstimatedTime(true);
    }

    public void OnFocus(GameObject gameobject)
    {
        RectTransform rect = gameobject.GetComponent<RectTransform>();
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        LeanTween.scale(rect, info.OriginalScale, LERP_TIME).setEase(info.EaseType).setUseEstimatedTime(true);
    }
}
