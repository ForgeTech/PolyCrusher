public class PauseQuitAction : LevelLoadByNameWithAudioActivation
{
    public override void PerformAction<T>(T triggerInstance)
    {
        // TODO: Implement reset method for DataCollector in order to get the pause menu quit to work.
        // DataCollector.instance.endSession();
        
        // TODO: Call steam manager reset method

        base.PerformAction<T>(triggerInstance);
    }
}