using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SubSelectionArrowHelper
{
    private Image leftArrow;
    private Image rightArrow;

    private Image leftArrowApply;
    private Image rightArrowApply;

    private float tweenTime;
    private LeanTweenType easeType;

    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;
    
    public SubSelectionArrowHelper(Image leftArrow, Image rightArrow,
        Image leftArrowApply, Image rightArrowApply, LeanTweenType easeType,
        float tweenTime, AbstractMenuManager menuManager, SelectorWithSubSelector selector)
    {
        this.leftArrow = leftArrow;
        this.rightArrow = rightArrow;

        this.leftArrowApply = leftArrowApply;
        this.rightArrowApply = rightArrowApply;

        this.tweenTime = tweenTime;
        this.easeType = easeType;

        this.menuManager = menuManager;
        this.selector = selector;
    }

    public void RepositionArrows()
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

    public void DoLeftArrowSizeTween()
    {
        SubSelector subSelectorOfCurrentObject = selector.SubSelectionEntries[selector.Components[selector.Current]];
        if (subSelectorOfCurrentObject.Components.Count > 0)
            DoSizeTween(leftArrow);
    }

    public void DoRightArrowSizeTween()
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
