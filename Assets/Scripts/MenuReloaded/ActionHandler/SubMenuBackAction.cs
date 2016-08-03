using UnityEngine;
using System.Collections;
using System;

public class SubMenuBackAction : AbstractActionHandler
{
    public override void PerformAction<T>(T triggerInstance)
    {
        OnActionPerformed();
    }
}
