using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A special selector which does its selection logic based on the CharacterSelectionHelper.
/// </summary>
public class CharacterSelector : Selector
{
    private static int NULL_SELECTION = -1;
    private CharacterSelectionHelper selectionHelper;
    private CharacterMenuManager menuManager;
    private int selectedIndex = NULL_SELECTION;

    public CharacterSelector(int startIndex, Dictionary<int, GameObject> components,
        TransitionHandlerInterface[] transitionHandlers,
        ElementPressedHandler[] pressedHandler,
        CharacterSelectionHelper selectionHelper, CharacterMenuManager menuManager, bool initialFocus) 
        : base(startIndex, components, transitionHandlers, pressedHandler, initialFocus)
    {
        this.selectionHelper = selectionHelper;
        this.menuManager = menuManager;
    }

    public override void HandleSelectedElement()
    {
        bool isAlreadySelected = selectionHelper.SelectionMap[Current].selected;

        if (!isAlreadySelected)             // Normal selection case
        {
            HandleCharacterSelected(Current);
            selectionHelper.SelectAt(Current, menuManager.PlayerSlot);
        }
        else if (selectedIndex == Current)  // Deselection case
        {
            HandleCharacterSelected(NULL_SELECTION);
            selectionHelper.DeselectAt(Current);
        }
    }

    protected override void OnNext()
    {
        CharacterSelectionHelper.SelectionData slot = selectionHelper.SelectionMap[Current];
        if (!slot.selected || slot.selectedBySlot != menuManager.PlayerSlot)
            base.OnNext();
    }

    protected override void OnPrevious()
    {
        CharacterSelectionHelper.SelectionData slot = selectionHelper.SelectionMap[Current];
        if (!slot.selected || slot.selectedBySlot != menuManager.PlayerSlot)
            base.OnPrevious();
    }

    private void HandleCharacterSelected(int selectedIndex)
    {
        // Deselection
        if (selectedIndex == NULL_SELECTION)
        {
            DoSelectionRoutine(selectedIndex);
        }
        else
        {
            PlayerSlot slot = selectionHelper.SelectionMap[selectedIndex].selectedBySlot;
            
            // Only handle selection if the same menu is accesing it (if already selected) or if nothing at all is selected.
            if (slot == menuManager.PlayerSlot || slot == PlayerSlot.None)
                DoSelectionRoutine(selectedIndex);
        }
    }

    private void DoSelectionRoutine(int selectedIndex)
    {
        menuManager.SwitchNavigationActivationState();
        this.selectedIndex = selectedIndex;
        base.HandleSelectedElement();
    }
}