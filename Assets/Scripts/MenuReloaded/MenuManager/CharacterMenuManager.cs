using UnityEngine;
using System.Collections;

public class CharacterMenuManager : MenuManager
{
    [SerializeField]
    private CharacterSelectionHelper selectionHelper;

    protected override void Start()
    {
        base.Start();
        InitializeComponentSize();

        if (selectionHelper == null)
            Debug.LogError("Selection helper is not assigned!");
    }

    private void InitializeComponentSize()
    {
        foreach (var pair in components)
        {
            if (pair.Key != Selector.Current)
                pair.Value.GetComponent<RectTransform>().localScale = Vector3.zero;
        }
    }

    protected override void InitializeSelector()
    {
        TransitionHandlerInterface[] pickedTransitions = MenuReloadedUtil.MapTransitionEnumToHandler(transitions);
        ElementPressedHandler[] pickedPressedHandler = MenuReloadedUtil.MapElementPressedEnumToHandler(pressedHandler);

        selector = new CharacterSelector(startIndex, components, pickedTransitions, pickedPressedHandler, selectionHelper, this);
    }
}