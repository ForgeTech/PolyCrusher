using UnityEngine;

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

    [SerializeField]
    private Vector3 originalScale = Vector3.one;

    [SerializeField]
    private Vector3 deselectedScale = Vector3.one * 0.9f;

    [SerializeField]
    private Vector3 pressedScale = Vector3.one;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;
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

    public Vector3 OriginalScale
    {
        get { return this.originalScale; }
    }

    public Vector3 DeselectedScale
    {
        get { return this.deselectedScale; }
    }

    public Vector3 PressedScale
    {
        get { return this.pressedScale; }
    }

    public LeanTweenType EaseType
    {
        get { return this.easeType; }
    }
    #endregion
}