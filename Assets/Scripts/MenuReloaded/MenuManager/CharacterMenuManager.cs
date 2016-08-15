using UnityEngine;

public class CharacterMenuManager : MenuManager
{
    [SerializeField]
    private CharacterSelectionHelper selectionHelper;

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
            // TODO: 
        }
        else
        {
            // TODO: 
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

    public override void InitializeMenuManager()
    {
        // TODO: Only for testing 
        base.InitializeMenuManager();

        //InitializeDictionary();
        //InitializeSelector();
        //// Create initial null binding -> The control binding is set by another script
        //SetPlayerControlActions(PlayerControlActions.CreateNullBinding());
    }

    protected override void InitializeSelector()
    {
        TransitionHandlerInterface[] pickedTransitions = MenuReloadedUtil.MapTransitionEnumToHandler(transitions);
        ElementPressedHandler[] pickedPressedHandler = MenuReloadedUtil.MapElementPressedEnumToHandler(pressedHandler);

        selector = new CharacterSelector(startIndex, components, pickedTransitions, pickedPressedHandler, selectionHelper, this);
    }
}