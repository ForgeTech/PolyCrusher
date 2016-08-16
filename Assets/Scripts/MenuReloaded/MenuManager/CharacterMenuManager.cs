using UnityEngine;

public class CharacterMenuManager : MenuManager
{
    [SerializeField]
    private PlayerSlot playerSlot;

    [SerializeField]
    private CharacterSelectionHelper selectionHelper;

    public PlayerSlot PlayerSlot
    {
        get { return this.playerSlot; }
    }

    protected override void Start()
    {
        base.Start();
        PlayerActionChanged += HandlePlayerActionChanged;

        InitializeComponentSize();

        if (selectionHelper == null)
            Debug.LogError("Selection helper is not assigned!");
    }

    private void HandlePlayerActionChanged(PlayerControlActions playerAction)
    {
        if (playerAction.IsNullAction())
        {
            // TODO: Don't show menu -> Controller not registered
        }
        else
        {
            // TODO: Fade in menu -> Controller registered
        }
    }

    private void InitializeComponentSize()
    {
        foreach (var pair in components)
        {
            if (pair.Key != Selector.Current)
                pair.Value.GetComponent<RectTransform>().localScale = Vector3.zero;
        }
    }

    protected override void InitializePlayerControlActions()
    {
        SetPlayerControlActions(PlayerControlActions.CreateNullBinding());
    }

    protected override void InitializeSelector()
    {
        TransitionHandlerInterface[] pickedTransitions = MenuReloadedUtil.MapTransitionEnumToHandler(transitions);
        ElementPressedHandler[] pickedPressedHandler = MenuReloadedUtil.MapElementPressedEnumToHandler(pressedHandlerEnum);

        selector = new CharacterSelector(startIndex, components, pickedTransitions, pickedPressedHandler, selectionHelper, this);
    }
}