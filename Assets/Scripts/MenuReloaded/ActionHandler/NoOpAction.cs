using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This action handler does absolutely nothing.
/// </summary>
public class NoOpAction : AbstractActionHandler
{
    public override void PerformAction<T>(T triggerInstance)
    {
        Debug.Log("You did nothing! :P");
    }
}
