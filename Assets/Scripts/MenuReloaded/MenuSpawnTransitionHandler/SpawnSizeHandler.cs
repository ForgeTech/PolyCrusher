using System.Collections.Generic;
using UnityEngine;

public class SpawnSizeHandler : MenuSpawnTransitionHandler
{
    private readonly float tweenTime;
    private readonly LeanTweenType easeType;

    public SpawnSizeHandler(float tweenTime, LeanTweenType easeType)
    {
        this.tweenTime = tweenTime;
        this.easeType = easeType;
    }

    public void HandleMenuSpawnTransition(Dictionary<int, GameObject> components, SelectorInterface selector)
    {
        foreach (var pair in components)
        {
            NavigationInformation info = pair.Value.GetComponent<NavigationInformation>();
            RectTransform rect = pair.Value.GetComponent<RectTransform>();

            rect.localScale = Vector3.zero;

            if(pair.Key == selector.Current)
                LeanTween.scale(rect, info.OriginalScale, tweenTime).setEase(easeType).setUseEstimatedTime(true);
            else
                LeanTween.scale(rect, info.DeselectedScale, tweenTime).setEase(easeType).setUseEstimatedTime(true);
        }
    }
}