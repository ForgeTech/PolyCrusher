using UnityEngine;

/// <summary>
/// Interface for all interactive UI-Elements which can be pressed.
/// </summary>
public interface ElementPressedHandler
{
    /// <summary>
    /// Should be called when the specific element is pressed.
    /// </summary>
    /// <param name="pressedGameObject">GameObject which was pressed.</param>
    void ElementPressed(GameObject pressedGameObject);
}