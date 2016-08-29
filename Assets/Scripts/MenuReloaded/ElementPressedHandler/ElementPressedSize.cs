using UnityEngine;

public class ElementPressedSize : ElementPressedHandler
{
    // In ms
    private const float LERP_TIME = 0.2f;

    public void ElementPressed(GameObject pressedGameObject)
    {
        RectTransform rect = pressedGameObject.GetComponent<RectTransform>();
        NavigationInformation info = pressedGameObject.GetComponent<NavigationInformation>();

        float halfLerpTime = LERP_TIME * 0.5f;

        LeanTween.scale(rect, info.PressedScale, halfLerpTime).setUseEstimatedTime(true).setEase(LeanTweenType.easeOutSine)
            .setOnComplete(() => {
                LeanTween.scale(rect, Vector3.one, halfLerpTime).setUseEstimatedTime(true).setEase(LeanTweenType.easeOutSine);
            });
    }
}