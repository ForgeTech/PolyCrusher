using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AbstractMenuManager))]
public class LeaderboardHelper : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    private GameObject dataRow;

    [SerializeField]
    private RectTransform highscoreContainer;

    [Header("Arrows")]
    [SerializeField]
    private Image leftArrow;
    [SerializeField]
    private Image rightArrow;

    [SerializeField]
    private Image leftArrowApply;
    [SerializeField]
    private Image rightArrowApply;

    [Header("Tween settings")]
    [SerializeField]
    private LeanTweenType easeType = LeanTweenType.easeOutSine;

    [SerializeField]
    private float tweenTime = 0.2f;
    #endregion

    #region Internal members
    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;
    private SubSelectionArrowHelper arrowTweenHelper;
    #endregion

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        menuManager = GetComponent<AbstractMenuManager>();
        selector = (SelectorWithSubSelector)menuManager.Selector;

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
