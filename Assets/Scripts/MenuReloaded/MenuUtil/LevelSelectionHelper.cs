using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A helper which is an addition to the AbstractMenuManager.
/// It should be only used for the level selection.
/// The script handles the placement of the level islands and the corresponding tweens,
/// the whole selection is part of the AbstractMenuManager.
/// </summary>
[RequireComponent(typeof(AbstractMenuManager))]
public class LevelSelectionHelper : MonoBehaviour
{
    #region Inspector variables
    [Header("Settings")]
    [SerializeField]
    private float gapSize = 660f;

    [SerializeField]
    private Vector3 middlePoint = Vector3.zero;

    [Header("UI Elements")]
    [SerializeField]
    private Image leftArrow;

    [SerializeField]
    private Image rightArrow;

    [SerializeField]
    private Color pressedColor;

    [Header("Tweening options")]
    [SerializeField]
    private float tweenTime = 0.2f;

    [SerializeField]
    private float scaleTweenMultiplier = 2f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutExpo;

    [SerializeField]
    private LeanTweenType levelTransitionEaseType = LeanTweenType.easeOutExpo;
    #endregion

    #region Internal members
    private AbstractMenuManager menuManager;
    private SelectorInterface selector;
    private RectTransform[] levelIslands;
    private Color originalArrowColor;
    #endregion

    private Vector2 tweenOutPosition = new Vector2(0f, 5000f);

	private void Start ()
    {
        Initialize();
	}

    private void Initialize()
    {
        // Initialize the menu manager and the selector
        menuManager = GetComponent<AbstractMenuManager>();
        selector = menuManager.Selector;
        menuManager.NavigationNext += HandleNextSelection;
        menuManager.NavigationPrevious += HandlePreviousSelection;

        levelIslands = new RectTransform[menuManager.MenuComponents.Count];

        // Fill the internal level array
        for (int i = 0; i < levelIslands.Length; i++)
        {
            GameObject tmp;
            menuManager.MenuComponents.TryGetValue(i, out tmp);
            levelIslands[i] = tmp.GetComponent<RectTransform>();
        }

        // Other initialization steps
        originalArrowColor = leftArrow.color;
        RepositionElements();
    }

    private void RepositionElements()
    {
        Vector3 offset = new Vector3(gapSize, 0f, 0f);

        for (int i = 0; i < levelIslands.Length; i++)
        {
            int index;

            if (i == (index = CalculateIndex(selector.Current - 2)))
                DoReposition(index, middlePoint - offset * 2f);
            else if (i == (index = CalculateIndex(selector.Current - 1)))
                DoReposition(index, middlePoint - offset);
            else if (i == selector.Current)
                DoReposition(selector.Current, middlePoint);
            else if (i == (index = CalculateIndex(selector.Current + 1)))
                DoReposition(index, middlePoint + offset);
            else if (i == (index = CalculateIndex(selector.Current + 2)))
                DoReposition(index, middlePoint + offset * 2f);
            else
                levelIslands[i].anchoredPosition = tweenOutPosition;
        }
    }

    private void DoReposition(int index, Vector2 offset)
    {
        RectTransform island = levelIslands[index];
        TweenLevelIsland(island, island.anchoredPosition, offset);
    }

    private void TweenLevelIsland(RectTransform island, Vector2 from, Vector2 to)
    {
        LeanTween.value(island.gameObject, from, to, tweenTime)
            .setEase(levelTransitionEaseType)
            .setOnUpdate((Vector2 val) => {
                island.anchoredPosition = val;
            });
    }

    private int CalculateIndex(int index)
    {
        return CalculatePositiveMod(index, levelIslands.Length);
    }

    private int CalculatePositiveMod(int a, int n)
    {
        return ((a % n) + n) % n;
    }

    /// <summary>
    /// Callback which is registered at the AbstractMenuManager.
    /// </summary>
    private void HandleNextSelection()
    {
        TweenArrow(rightArrow);
        RepositionElements();
    }

    /// <summary>
    /// Callback which is registered at the AbstractMenuManager.
    /// </summary>
    private void HandlePreviousSelection()
    {
        TweenArrow(leftArrow);
        RepositionElements();
    }

    private void TweenArrow(Image arrow)
    {
        if (leftArrow != null)
        {
            #region Color Tween
            LeanTween.value(arrow.gameObject, originalArrowColor, pressedColor, tweenTime * 0.5f)
                .setEase(easeType)
                .setOnUpdate((Color val) => {
                    arrow.color = val;
                })
                .setOnComplete(
                () => {
                    Color c = new Color(arrow.color.r, arrow.color.g, arrow.color.b);
                    LeanTween.value(arrow.gameObject, c, originalArrowColor, tweenTime * 0.5f)
                    .setEase(easeType)
                    .setOnUpdate((Color val) => {
                        arrow.color = val;
                    });
                });
            #endregion

            #region Position Tween
            LeanTween.scale(arrow.rectTransform, Vector3.one * scaleTweenMultiplier, tweenTime * 0.5f)
                .setEase(easeType)
                .setOnComplete(() => {
                    LeanTween.scale(arrow.rectTransform, Vector3.one, tweenTime * 0.5f).setEase(easeType);
                });
            #endregion
        }
        else
            Debug.LogError("Left arrow image not assigned!");
    }
}