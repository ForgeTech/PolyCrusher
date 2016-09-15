using UnityEngine;
using System.Collections;

public class LevelLoadScoreMenu : LevelLoadByName
{
    private bool levelLoadAllowed = false;
       
    public override void PerformAction<T>(T triggerInstance)
    {
        if (levelLoadAllowed)
            base.PerformAction<T>(triggerInstance);
        else
            OnActionPerformed();

        levelLoadAllowed = true;
    }
}