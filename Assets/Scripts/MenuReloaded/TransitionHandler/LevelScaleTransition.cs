using UnityEngine;

public class LevelScaleTransition : TransitionHandlerInterface
{
    // In ms
    private const float LERP_TIME = 0.2f;
    const float DE_SELECTION_SIZE = 0.6f;

    public void OnDefocus(GameObject gameobject)
    {
        RectTransform rect = gameobject.GetComponent<RectTransform>();
        LeanTween.scale(rect, Vector3.one * DE_SELECTION_SIZE, LERP_TIME).setEase(LeanTweenType.easeOutSine);
    }

    public void OnFocus(GameObject gameobject)
    {
        RectTransform rect = gameobject.GetComponent<RectTransform>();
        LeanTween.scale(rect, Vector3.one, LERP_TIME).setEase(LeanTweenType.easeOutSine);
    }
}