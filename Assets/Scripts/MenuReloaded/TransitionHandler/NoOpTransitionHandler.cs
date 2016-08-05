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
        Debug.Log("NoOp Transition Defocus.");
    }

    public void OnFocus(GameObject gameobject)
    {
        Debug.Log("NoOp Transition Focus.");
    }
}