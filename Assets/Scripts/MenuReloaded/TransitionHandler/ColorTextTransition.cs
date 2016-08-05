using UnityEngine;
using UnityEngine.UI;

public class ColorTextTransition : TransitionHandlerInterface
{
    // In ms
    private const float LERP_TIME = 0.2f;

    public void OnDefocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        Text text = gameobject.GetComponent<Text>();
        Color startColor = new Color(text.color.r, text.color.g, text.color.b);

        LeanTween.value(gameobject, startColor, info.NormalColor, LERP_TIME).setOnUpdate(
            (Color val) => { text.color = val; }
        ).setEase(LeanTweenType.easeInQuad);
    }

    public void OnFocus(GameObject gameobject)
    {
        NavigationInformation info = gameobject.GetComponent<NavigationInformation>();
        Text text = gameobject.GetComponent<Text>();
        Color startColor = new Color(text.color.r, text.color.g, text.color.b);

        LeanTween.value(gameobject, startColor, info.HighlightedColor, LERP_TIME).setOnUpdate(
            (Color val) => { text.color = val; }
        ).setEase(LeanTweenType.easeInQuad);
    }
}
