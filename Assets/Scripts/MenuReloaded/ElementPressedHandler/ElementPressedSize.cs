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
        float halfLerpTime = LERP_TIME * 0.5f;

        LeanTween.scale(rect, new Vector2(SIZE_MULTIPLIER, SIZE_MULTIPLIER), halfLerpTime)
            .setOnComplete(() => {
                LeanTween.scale(rect, Vector2.one, halfLerpTime);
            });
    }
}