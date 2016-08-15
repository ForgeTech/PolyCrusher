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
        CharacterSelectionHelper selectionHelper, AbstractMenuManager menuManager) 
        : base(startIndex, components, transitionHandlers, pressedHandler)
    {
        this.selectionHelper = selectionHelper;
        this.menuManager = menuManager;
    }

    public override void HandleSelectedElement()
    {
        bool isAlreadySelected = selectionHelper.SelectionMap[Current];

        if (!isAlreadySelected)             // Normal selection case
            HandleCharacterSelected(Current);
        else if (selectedIndex == Current)  // Deselection case
            HandleCharacterSelected(NULL_SELECTION);
    }

    private void HandleCharacterSelected(int selectedIndex)
    {
        base.HandleSelectedElement();
        this.selectedIndex = selectedIndex;
        menuManager.SwitchNavigationActivationState();
    }
}