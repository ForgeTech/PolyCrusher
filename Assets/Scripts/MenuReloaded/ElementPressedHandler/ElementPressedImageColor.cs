using UnityEngine;
using UnityEngine.UI;

public class ElementPressedImageColor : ElementPressedHandler
{
    // In ms
    const float LERP_TIME = 0.2f;

    public void ElementPressed(GameObject pressedGameObject)
    {
        Image img = pressedGameObject.GetComponent<Image>();
        NavigationInformation navigationInfo = pressedGameObject.GetComponent<NavigationInformation>();
        if (img != null)
        {
            Color originalColor = new Color(img.color.r, img.color.g, img.color.b);
            LeanTween.value(pressedGameObject, originalColor, navigationInfo.PressedColor, LERP_TIME)
                .setEase(LeanTweenType.easeInCubic)
                .setOnUpdate((Color val) => {
                    img.color = val;
                })
                .setOnComplete(
                () => {
                    Color c = new Color(img.color.r, img.color.g, img.color.b);
                    LeanTween.value(pressedGameObject, c, navigationInfo.HighlightedColor, LERP_TIME)
                    .setEase(LeanTweenType.easeInCubic)
                    .setOnUpdate((Color val) => {
                        img.color = val;
                    });
                });
        }
        else
            Debug.LogError("No Image component found!");
    }
}
