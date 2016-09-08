using UnityEngine;

public class PauseQuitAction : LevelLoadByNameWithAudioActivation
{
    public override void PerformAction<T> (T triggerInstance)
    {
        BaseSteamManager.Instance.ResetGame();
        DataCollector.instance.Reset();

        LevelEndManager levelEndManager = GameObject.FindObjectOfType<LevelEndManager>();
        levelEndManager.OnLevelExit();
        base.PerformAction<T>(triggerInstance);
    }
}