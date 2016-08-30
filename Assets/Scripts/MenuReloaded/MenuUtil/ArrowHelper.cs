using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class for arrow movement in normal menus (like main menu or pause menu).
/// </summary>
[RequireComponent(typeof(AbstractMenuManager))]
public class ArrowHelper : MonoBehaviour
{
    private AbstractMenuManager menuManager;
    private SelectorInterface selector;

    [SerializeField]
    private Image leftArrow;

    [SerializeField]
    private Image rightArrow;

    [SerializeField]
    private float tweenTime = 0.2f;

    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

	private void Start ()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        selector = menuManager.Selector;
        menuManager.NavigationNext += HandleSelectionChange;
        menuManager.NavigationPrevious += HandleSelectionChange;
	}

    private void PositionArrows()
    {
        SetParentOfArrows();

        LeanTween.moveY(leftArrow.rectTransform, 0f, tweenTime).setEase(easeType).setUseEstimatedTime(true);
        LeanTween.moveY(rightArrow.rectTransform, 0f, tweenTime).setEase(easeType).setUseEstimatedTime(true);

        GameObject selectedComponent = GetCurrentlySelectedMenuElement();
        RectTransform selectedRect = selectedComponent.GetComponent<RectTransform>();

        LeanTween.moveX(leftArrow.rectTransform, -selectedRect.sizeDelta.x * 0.5f, tweenTime).setEase(easeType).setUseEstimatedTime(true);
        LeanTween.moveX(rightArrow.rectTransform, selectedRect.sizeDelta.x * 0.5f, tweenTime).setEase(easeType).setUseEstimatedTime(true);

        leftArrow.rectTransform.localScale = Vector3.one;
        rightArrow.rectTransform.localScale = Vector3.one;
    }

    private void SetParentOfArrows()
    {
        GameObject parentObject = GetCurrentlySelectedMenuElement();
        leftArrow.transform.SetParent(parentObject.transform, true);
        rightArrow.transform.SetParent(parentObject.transform, true);
    }

    private void HandleSelectionChange()
    {
        PositionArrows();
    }

    private GameObject GetCurrentlySelectedMenuElement()
    {
        return menuManager.MenuComponents[selector.Current];
    }
}