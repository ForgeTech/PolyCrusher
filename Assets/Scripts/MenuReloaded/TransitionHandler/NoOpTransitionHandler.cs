using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This transition handler does absolutely nothing!
/// </summary>
public class NoOpTransitionHandler : TransitionHandlerInterface
{
    public void OnDefocus(GameObject gameobject)
    {
        // Nothing to do here
    }

    public void OnFocus(GameObject gameobject)
    {
        // Nothing to do here
    }
}