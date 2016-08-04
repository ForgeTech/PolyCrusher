using UnityEngine;
using System;

public class ElementPressedSize : ElementPressedHandler
{
    // In ms
    private const float LERP_TIME = 0.1f;
    const float SIZE_MULTIPLIER = 1.1f;

    public void ElementPressed(GameObject pressedGameObject)
    {
        RectTransform rect = pressedGameObject.GetComponent<RectTransform>();
        LeanTween.scale(rect, new Vector2(SIZE_MULTIPLIER, SIZE_MULTIPLIER), LERP_TIME).setEase(LeanTweenType.easeInOutBounce).setLoopPingPong(1);
    }
}