using UnityEngine;

public class CharacterSizePressed : ElementPressedHandler
{
    private bool isPressed = false;

    public void ElementPressed(GameObject pressedGameObject)
    {
        isPressed = !isPressed;
        NavigationInformation info = pressedGameObject.GetComponent<NavigationInformation>();
        RectTransform rect = pressedGameObject.GetComponent<RectTransform>();

        if (isPressed)
            LeanTween.scale(rect, Vector3.one, 0.15f).setEase(info.EaseType);
        else
            LeanTween.scale(rect, info.OriginalScale, 0.15f).setEase(info.EaseType);
    }
}
