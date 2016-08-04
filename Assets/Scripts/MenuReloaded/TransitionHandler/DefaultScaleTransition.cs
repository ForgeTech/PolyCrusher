using UnityEngine;
using System.Collections;
using System;

public class DefaultScaleTransition : TransitionHandlerInterface
{
    // In ms
    private const float LERP_TIME = 0.2f;
    const float SIZE_MULTIPLIER = 1.3f;

    public void OnDefocus(GameObject gameobject)
    {
        // Do nothing
    }

    public void OnFocus(GameObject gameobject)
    {
        RectTransform rect = gameobject.GetComponent<RectTransform>();
        LeanTween.scale(rect, new Vector2(SIZE_MULTIPLIER, SIZE_MULTIPLIER), LERP_TIME).setEase(LeanTweenType.easeInBounce).setLoopPingPong(1);
    }
}
