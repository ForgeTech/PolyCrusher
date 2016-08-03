using UnityEngine;
using System.Collections;
using System;

public class TestAction : AbstractActionHandler
{
    public override void PerformAction<T>(T triggerInstance)
    {
        Application.LoadLevel(9);
        OnActionPerformed();
    }
}
