using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AbstractMenuManager))]
public class LevelSelectionHelper : MonoBehaviour
{
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

    [SerializeField]
    private float tweenTime = 0.2f;

    [SerializeField]
    private float scaleTweenMultiplier = 2f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutExpo;

    [SerializeField]
    private LeanTweenType levelTransitionEaseType = LeanTweenType.easeOutExpo;

    private AbstractMenuManager menuManager;
    private SelectorInterface selector;
    private RectTransform[] levelIslands;
    private Color originalArrowColor;

	private void Start ()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        selector = menuManager.Selector;
        menuManager.NavigationNext += HandleNextSelection;
        menuManager.NavigationPrevious += HandlePreviousSelection;

        levelIslands = new RectTransform[menuManager.MenuComponents.Count];

        for (int i = 0; i < levelIslands.Length; i++)
        {
            GameObject tmp;
            menuManager.MenuComponents.TryGetValue(i, out tmp);
            levelIslands[i] = tmp.GetComponent<RectTransform>();
        }

        originalArrowColor = leftArrow.color;

        RePositionElements();
	}

    private void RePositionElements()
    {
        Vector3 offset = new Vector3(gapSize, 0f, 0f);

        for (int i = 0; i < levelIslands.Length; i++)
        {
            RectTransform island;
            if (i == CalculateIndex(selector.Current - 1))
            {
                island = levelIslands[CalculateIndex(selector.Current - 1)];
                TweenLevelIsland(island, island.anchoredPosition, middlePoint - offset);

                //levelIslands[CalculateIndex(selector.Current - 1)].anchoredPosition = middlePoint - offset;
            }
            else if (i == selector.Current)
            {
                island = levelIslands[selector.Current];
                TweenLevelIsland(island, island.anchoredPosition, middlePoint);

                //levelIslands[selector.Current].anchoredPosition = middlePoint;
            }
            else if (i == CalculateIndex(selector.Current + 1))
            {
                island = levelIslands[CalculateIndex(selector.Current + 1)];
                TweenLevelIsland(island, island.anchoredPosition, middlePoint + offset);

                //levelIslands[CalculateIndex(selector.Current + 1)].anchoredPosition = middlePoint + offset;
            }
            else
            {
                island = levelIslands[i];
                TweenLevelIsland(island, island.anchoredPosition, new Vector2(0, 1200f));
            }
        }
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

    private void HandleNextSelection()
    {
        TweenArrow(rightArrow);
        RePositionElements();
    }

    private void HandlePreviousSelection()
    {
        TweenArrow(leftArrow);
        RePositionElements();
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