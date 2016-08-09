using UnityEngine;
using UnityEngine.UI;

public class ElementPressedTextColor : ElementPressedHandler
{
    // In ms
    const float LERP_TIME = 0.2f;

    public void ElementPressed(GameObject pressedGameObject)
    {
        Text text = pressedGameObject.GetComponent<Text>();
        NavigationInformation navigationInfo = pressedGameObject.GetComponent<NavigationInformation>();
        if (text != null)
        {
            Color originalColor = new Color(text.color.r, text.color.g, text.color.b);
            LeanTween.value(pressedGameObject, originalColor, navigationInfo.PressedColor, LERP_TIME)
                .setEase(LeanTweenType.easeOutSine)
                .setOnUpdate((Color val) => {
                    text.color = val;
                })
                .setOnComplete(
                () => {
                    Color c = new Color(text.color.r, text.color.g, text.color.b);
                    LeanTween.value(pressedGameObject, c, navigationInfo.HighlightedColor, LERP_TIME)
                    .setEase(LeanTweenType.easeOutSine)
                    .setOnUpdate((Color val) => {
                        text.color = val;
                    });
                });
        }
        else
            Debug.LogError("No Text component found!");
    }
}
