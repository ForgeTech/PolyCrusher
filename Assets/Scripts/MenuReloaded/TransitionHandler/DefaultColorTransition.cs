using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class DefaultColorTransition : TransitionHandlerInterface
{
    // In ms
    private const float LERP_TIME = 0.2f;

    public void OnDefocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        Image image = gameobject.GetComponent<Image>();
        Color startColor = new Color(image.color.r, image.color.g, image.color.b);

        LeanTween.value(gameobject, startColor, info.NormalColor, LERP_TIME).setOnUpdate(
            (Color val) => { image.color = val; }
        ).setEase(LeanTweenType.easeInQuad);
    }

    public void OnFocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        Image image = gameobject.GetComponent<Image>();
        Color startColor = new Color(image.color.r, image.color.g, image.color.b);

        LeanTween.value(gameobject, startColor, info.HighlightedColor, LERP_TIME).setOnUpdate(
            (Color val) => { image.color = val; }
        ).setEase(LeanTweenType.easeInQuad);
    }
}