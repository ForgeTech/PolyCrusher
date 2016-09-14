using System.Collections.Generic;
using UnityEngine;

public class SubSelector : Selector
{
    private TransitionHandlerInterface[] subSelectorTransitions;

    public SubSelector(int startIndex, Dictionary<int, GameObject> components,
        TransitionHandlerInterface[] transitionHandlers,
        ElementPressedHandler[] pressedHandler,
        bool initialFocus, TransitionHandlerInterface[] subSelectorTransition) 
        : base(startIndex, components, transitionHandlers, pressedHandler, initialFocus)
    {
        this.subSelectorTransitions = subSelectorTransition;

        if(Components.Count > 0)
            Components[startIndex].transform.localScale = Vector3.one;
    }

    internal void InvokeTransitionDeFocus()
    {
        foreach (TransitionHandlerInterface t in subSelectorTransitions)
            t.OnDefocus(Components[Current]);
    }

    internal void InvokeTransitionFocus()
    {
        foreach (TransitionHandlerInterface t in subSelectorTransitions)
            t.OnFocus(Components[Current]);
    }
}