using UnityEngine;
using System.Collections.Generic;

public class SelectorWithSubSelector : Selector
{
    private readonly Dictionary<GameObject, SubSelector> subSelectionEntries = new Dictionary<GameObject, SubSelector>();

    /// <summary>
    /// Gets the Sub selector of the current selected menu element.
    /// </summary>
    public SubSelector SubSelector
    {
        get
        {
            GameObject g;
            components.TryGetValue(Current, out g);

            return subSelectionEntries[g];
        }
    }

    public SelectorWithSubSelector(int startIndex, Dictionary<int, GameObject> components, TransitionHandlerInterface[] transitionHandlers, ElementPressedHandler[] pressedHandler, bool initialFocus, TransitionHandlerInterface[] subSelectorTransition) 
        : base(startIndex, components, transitionHandlers, pressedHandler, initialFocus)
    {
        InitializeEntries(components, subSelectorTransition);
    }

    private void InitializeEntries(Dictionary<int, GameObject> components, TransitionHandlerInterface[] subSelectorTransition)
    {
        foreach (var pair in components)
        {
            bool initialFocus = pair.Key == Current;

            Dictionary<int, GameObject> subSelecorComponents = new Dictionary<int, GameObject>();
            foreach(Transform child in pair.Value.transform)
            {
                NavigationInformation info = child.gameObject.GetComponent<NavigationInformation>();
                subSelecorComponents.Add(info.SelectionID, child.gameObject);
            }

            subSelectionEntries.Add(pair.Value,
                new SubSelector(0, subSelecorComponents, this.transitionHandler, this.elementPressedHandler, initialFocus, subSelectorTransition));
        }
    }

    public void SubSelectorNext()
    {
        SubSelector.Next();
    }

    public void SubSelectorPrevious()
    {
        SubSelector.Previous();
    }

    protected override void BeforeSelection(GameObject currentElement)
    {
        base.BeforeSelection(currentElement);
        if (SubSelector.Components.Count > 0)
            SubSelector.InvokeTransitionDeFocus();
    }

    protected override void AfterSelection(GameObject currentElement)
    {
        base.AfterSelection(currentElement);
        if (SubSelector.Components.Count > 0)
            SubSelector.InvokeTransitionFocus();
    }
}