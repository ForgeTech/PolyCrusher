using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Default selector which scrolls to the next index by incrementing or decrementing by one.
/// </summary>
public class Selector : AbstractSelector
{
    private int minValue;
    private int maxValue;

    public Selector(int startIndex, Dictionary<int, GameObject> components, TransitionHandlerInterface transitionHandler) 
        : base(startIndex, components, transitionHandler)
    {
        FindMinAndMaxKey();
    }

    private void FindMinAndMaxKey()
    {       
        foreach(var pair in components)
        {
            maxValue = Math.Max(maxValue, pair.Key);
            minValue = Math.Min(minValue, pair.Key);
        }
    }

    protected override void OnNext()
    {
        if (!CheckIndex(++Current))
        {
            Current = minValue;
        }
    }

    protected override void OnPrevious()
    {
        if (!CheckIndex(--Current))
        {
            Current = maxValue;
        }
    }
}