using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Requires the image fields of the NavigationInformation to be set!
/// </summary>
public class ElementPressedImageHandler : ElementPressedHandler
{
    private bool isPressed = false;

    public void ElementPressed(GameObject pressedGameObject)
    {
        isPressed = !isPressed;
        NavigationInformation info = pressedGameObject.GetComponent<NavigationInformation>();
        Image img = pressedGameObject.GetComponent<Image>();

        if (isPressed)
            img.sprite = info.SelectedImage;
        else
            img.sprite = info.DeselectedImage;
    }
}
