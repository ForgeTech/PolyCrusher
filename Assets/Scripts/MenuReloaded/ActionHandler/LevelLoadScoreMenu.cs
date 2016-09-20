using UnityEngine;
using System.Collections;

public class LevelLoadScoreMenu : LevelLoadByName
{
    private bool levelLoadAllowed = false;
    private ScoreMenuHelper scoreHelper;

    private void Start()
    {
        scoreHelper = FindObjectOfType<ScoreMenuHelper>();
    }
       
    public override void PerformAction<T>(T triggerInstance)
    {
        if (levelLoadAllowed || scoreHelper.endOfAnimationReached)
            base.PerformAction<T>(triggerInstance);
        else
            OnActionPerformed();

        levelLoadAllowed = true;
    }
}