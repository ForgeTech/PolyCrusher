using UnityEngine;

/// <summary>
/// This element pressed handler does absolutely nothing.
/// </summary>
public class ElementPressedNoOp : ElementPressedHandler
{
    public void ElementPressed(GameObject pressedGameObject)
    {
        Debug.Log("Element Pressed does nothing!");
    }
}
