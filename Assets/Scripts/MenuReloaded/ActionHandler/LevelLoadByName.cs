using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Loads level by name.
/// </summary>
public class LevelLoadByName : AbstractActionHandler
{
    [SerializeField]
    protected string levelName = null;

    public override void PerformAction<T>(T triggerInstance)
    {
        Time.timeScale = 1f;

        if (levelName != null)
            Application.LoadLevel(levelName);
        else
            Debug.LogError("Level name is null!");

        OnActionPerformed();
    }
}