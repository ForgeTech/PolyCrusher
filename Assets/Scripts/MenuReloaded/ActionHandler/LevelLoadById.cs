using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Level load action by the level ID.
/// </summary>
public class LevelLoadById : AbstractActionHandler
{
    [SerializeField]
    private int levelId = 0;

    public override void PerformAction<T>(T triggerInstance)
    {
        Application.LoadLevel(levelId);
        OnActionPerformed();
    }
}