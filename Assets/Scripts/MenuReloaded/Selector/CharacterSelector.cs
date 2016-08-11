using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterSelector : Selector
{
    private CharacterSelectionHelper selectionHelper;
    private AbstractMenuManager menuManager;
    private int selectedIndex = -1;

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
            HandleCharacterSelected(-1);
    }

    private void HandleCharacterSelected(int selectedIndex)
    {
        base.HandleSelectedElement();
        this.selectedIndex = selectedIndex;
        menuManager.SwitchNavigationActivationState();
    }
}