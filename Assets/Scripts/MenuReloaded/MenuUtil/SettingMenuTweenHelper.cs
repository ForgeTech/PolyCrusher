using UnityEngine;
using UnityEngine.UI;

public class SettingMenuTweenHelper : MonoBehaviour
{
    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;

    [SerializeField]
    private Image leftArrow;

    [SerializeField]
    private Image rightArrow;

    [Header("Tween settings")]
    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [SerializeField]
    private float tweenTime = 0.2f;

    [SerializeField]
    private Image leftArrowApply;

    [SerializeField]
    private Image rightArrowApply;

	private void Start ()
    {
        Initialize();
	}

    private void Initialize()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        selector = (SelectorWithSubSelector) menuManager.Selector;

        menuManager.NavigationNext += RepositionArrows;
        menuManager.NavigationPrevious += RepositionArrows;
        menuManager.SubNavigationNext += RepositionArrows;
        menuManager.SubNavigationPrevious += RepositionArrows;

        menuManager.SubNavigationNext += DoRightArrowSizeTween;
        menuManager.SubNavigationPrevious += DoLeftArrowSizeTween;
    }

    private void RepositionArrows()
    {
        SubSelector subSelectorOfCurrentObject = selector.SubSelectionEntries[selector.Components[selector.Current]];
        NavigationInformation arrowInfo = leftArrow.gameObject.GetComponent<NavigationInformation>();
        if (subSelectorOfCurrentObject.Components.Count > 0)
        {
            NavigationInformation applyArrow = leftArrowApply.gameObject.GetComponent<NavigationInformation>();

            if (leftArrow.color.a == 0f)
            {
                TweenArrowColor(leftArrowApply, applyArrow.NormalColor);
                TweenArrowColor(rightArrowApply, applyArrow.NormalColor);
            }

            TweenArrowColor(leftArrow, arrowInfo.HighlightedColor);
            TweenArrowColor(rightArrow, arrowInfo.HighlightedColor);

            RectTransform currentSelection = subSelectorOfCurrentObject.Components[subSelectorOfCurrentObject.Current].GetComponent<RectTransform>();
            float currentSelectionHalfWidth = currentSelection.sizeDelta.x * 0.5f;

            leftArrow.rectTransform.SetParent(currentSelection.parent, true);
            rightArrow.rectTransform.SetParent(currentSelection.parent, true);

            Vector2 leftArrowPosition = currentSelection.anchoredPosition - new Vector2(currentSelectionHalfWidth + leftArrow.rectTransform.sizeDelta.x, 0f);
            Vector2 rightArrowPosition = currentSelection.anchoredPosition + new Vector2(currentSelectionHalfWidth + rightArrow.rectTransform.sizeDelta.x, 0f);

            // Do ultra magic Tween!
            LeanTween.move(leftArrow.rectTransform, leftArrowPosition, tweenTime).setEase(easeType);
            LeanTween.move(rightArrow.rectTransform, rightArrowPosition, tweenTime).setEase(easeType);
        }
        else
        {
            if (leftArrow.color.a != 0f)
            {
                TweenArrowColor(leftArrow, arrowInfo.PressedColor);
                TweenArrowColor(rightArrow, arrowInfo.PressedColor);
            }

            NavigationInformation applyArrow = leftArrowApply.gameObject.GetComponent<NavigationInformation>();
            TweenArrowColor(leftArrowApply, applyArrow.PressedColor);
            TweenArrowColor(rightArrowApply, applyArrow.PressedColor);
        }
    }

    private void TweenArrowColor(Image arrow, Color c)
    {
        Color currentColor = new Color(arrow.color.r, arrow.color.g, arrow.color.b);
        LeanTween.value(arrow.gameObject, currentColor, c, tweenTime).setEase(easeType)
            .setOnUpdate((Color val) => {
                arrow.color = val;
            });
    }

    private void DoLeftArrowSizeTween()
    {
        SubSelector subSelectorOfCurrentObject = selector.SubSelectionEntries[selector.Components[selector.Current]];
        if(subSelectorOfCurrentObject.Components.Count > 0)
            DoSizeTween(leftArrow);
    }

    private void DoRightArrowSizeTween()
    {
        SubSelector subSelectorOfCurrentObject = selector.SubSelectionEntries[selector.Components[selector.Current]];
        if (subSelectorOfCurrentObject.Components.Count > 0)
            DoSizeTween(rightArrow);
    }

    private void DoSizeTween(Image image)
    {
        NavigationInformation info = image.gameObject.GetComponent<NavigationInformation>();

        LeanTween.scale(image.rectTransform, info.DeselectedScale, tweenTime * 0.5f)
            .setEase(easeType)
            .setOnComplete(() => {
                LeanTween.scale(image.rectTransform, info.OriginalScale, tweenTime * 0.5f).setEase(easeType);
            });
    }
}