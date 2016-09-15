using UnityEngine;
using System.Collections;

public class CreditMenuHelper : MonoBehaviour
{
    private enum Direction
    {
        Left = -1,
        Right = 1
    }

    private AbstractMenuManager menuManager;
    private bool isSlideShowActive = false;
    private PlayerControlActions playerActions;
    private DefaultMenuInputHandler inputHandler;

    [SerializeField]
    private RectTransform creditContainer;

    [SerializeField]
    private float maximumX;

    [SerializeField]
    private float slideShowSpeed = 5f;

    private void Start()
    {
        playerActions = PlayerControlActions.CreateWithGamePadBindings();
        inputHandler = new DefaultMenuInputHandler(playerActions);

        menuManager = GetComponent<AbstractMenuManager>();
        menuManager.ComponentSelected += SwitchSlideShowState;
    }

    private void SwitchSlideShowState(AbstractMenuManager triggerManager, GameObject selectedComponent)
    {
        isSlideShowActive = !isSlideShowActive;
    }

    private void DeactivateSlideshow()
    {
        isSlideShowActive = false;
    }

    private void TransformCredits()
    {
        if (isSlideShowActive && !IsMaximumPositionReached())
            TransformCreditContainer(Direction.Right);
    }

    private void TransformCreditContainer(Direction direction)
    {
        float x = creditContainer.anchoredPosition.x - Time.deltaTime * slideShowSpeed * (int) direction;
        creditContainer.anchoredPosition = new Vector2(x, creditContainer.anchoredPosition.y);
    }

    private bool IsMaximumPositionReached()
    {
        return Mathf.Abs(creditContainer.anchoredPosition.x) >= Mathf.Abs(maximumX);
    }

    private bool IsMinimumPositionReached()
    {
        return creditContainer.anchoredPosition.x >= 0;
    }

    private void HandleInput()
    {
        inputHandler.HandleHorizontalInput(
            () => { // Left input
                DeactivateSlideshow();

                if(!IsMinimumPositionReached())
                    TransformCreditContainer(Direction.Left);
            },  
            
            () => { // Right input
                DeactivateSlideshow();

                if (!IsMaximumPositionReached())
                    TransformCreditContainer(Direction.Right);
                else
                    BaseSteamManager.Instance.LogAchievementData(AchievementID.ACH_CREDITS_VIEWED);
            });
    }

    private void Update()
    {
        TransformCredits();
        HandleInput();
    }

    private void OnDestroy()
    {
        playerActions.Destroy();
    }
}