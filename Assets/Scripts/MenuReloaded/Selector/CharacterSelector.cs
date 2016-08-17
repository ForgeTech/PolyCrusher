using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A special selector which does its selection logic based on the CharacterSelectionHelper.
/// </summary>
public class CharacterSelector : Selector
{
    private static int NULL_SELECTION = -1;
    private CharacterSelectionHelper selectionHelper;
    private AbstractMenuManager menuManager;
    private int selectedIndex = NULL_SELECTION;

    public CharacterSelector(int startIndex, Dictionary<int, GameObject> components,
        TransitionHandlerInterface[] transitionHandlers,
        ElementPressedHandler[] pressedHandler,
        CharacterSelectionHelper selectionHelper, AbstractMenuManager menuManager, bool initialFocus) 
        : base(startIndex, components, transitionHandlers, pressedHandler, initialFocus)
    {
        this.selectionHelper = selectionHelper;
        this.menuManager = menuManager;
    }

    public override void HandleSelectedElement()
    {
        bool isAlreadySelected = selectionHelper.SelectionMap[Current];

        if (!isAlreadySelected)             // Normal selection case
        {
            HandleCharacterSelected(Current);
            selectionHelper.SelectAt(Current);
        }
        else if (selectedIndex == Current)  // Deselection case
        {
            HandleCharacterSelected(NULL_SELECTION);
            selectionHelper.DeselectAt(Current);
        }
    }

    protected override void OnNext()
    {
        if (!selectionHelper.SelectionMap[Current])
            base.OnNext();

        //// Backup routine which fixes a timing issue bug. (It may be that when the player selects often and then navigates, the count is not decremented since the deselection is not registered).
        //int lastIndex = Current;
        //base.OnNext();
        //if (selectionHelper.SelectionMap[lastIndex])
        //    selectionHelper.DeselectAt(lastIndex);
    }

    protected override void OnPrevious()
    {
        if (!selectionHelper.SelectionMap[Current])
            base.OnPrevious();
    }

    private void HandleCharacterSelected(int selectedIndex)
    {
        menuManager.SwitchNavigationActivationState();
        this.selectedIndex = selectedIndex;
        base.HandleSelectedElement();
    }
}