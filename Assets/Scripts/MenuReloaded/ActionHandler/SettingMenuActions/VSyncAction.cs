using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Enumeration for On and Off scenarios.
/// On  == 1 -> True
/// Off == 0 -> False
/// </summary>
public enum SwitchEnum
{
    On = 1,
    Off = 0
}

public class VSyncAction : AbstractActionHandler
{
    [SerializeField]
    public SwitchEnum vSyncValue = SwitchEnum.On;

    public override void PerformAction<T>(T triggerInstance)
    {
        if (vSyncValue == SwitchEnum.On)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;

        OnActionPerformed();
    }
}
