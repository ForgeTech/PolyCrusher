﻿using UnityEngine;
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

    private SubSelectionArrowHelper arrowTweenHelper;

	private void Start ()
    {
        Initialize();
	}

    private void Initialize()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        selector = (SelectorWithSubSelector) menuManager.Selector;

        arrowTweenHelper = new SubSelectionArrowHelper(leftArrow, rightArrow, leftArrowApply,
            rightArrowApply, easeType, tweenTime, menuManager, selector);

        // Event registration
        menuManager.NavigationNext += arrowTweenHelper.RepositionArrows;
        menuManager.NavigationPrevious += arrowTweenHelper.RepositionArrows;
        menuManager.SubNavigationNext += arrowTweenHelper.RepositionArrows;
        menuManager.SubNavigationPrevious += arrowTweenHelper.RepositionArrows;

        menuManager.SubNavigationNext += arrowTweenHelper.DoRightArrowSizeTween;
        menuManager.SubNavigationPrevious += arrowTweenHelper.DoLeftArrowSizeTween;
    }
}