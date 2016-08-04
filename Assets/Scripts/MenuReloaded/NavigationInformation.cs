using UnityEngine;
using System.Collections;

/// <summary>
/// Contains information about the navigation index of each interactive element
/// and the color of each press and selection state.
/// </summary>
public class NavigationInformation : MonoBehaviour
{
    #region Inspector variables
    [Header("Selection Information")]
    [SerializeField]
    private int selectionID;

    [Header("Button state colors")]
    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color highlightedColor = Color.red;

    [SerializeField]
    private Color pressedColor = Color.yellow;
    #endregion

    #region Properties
    public Color NormalColor
    {
        get { return this.normalColor; }
    }

    public Color HighlightedColor
    {
        get { return this.highlightedColor; }
    }

    public Color PressedColor
    {
        get { return this.pressedColor; }
    }

    /// <summary>
    /// The ID (or index) of the element.
    /// </summary>
    public int SelectionID
    {
        get { return selectionID; }
        set { this.selectionID = value; }
    }
    #endregion
}