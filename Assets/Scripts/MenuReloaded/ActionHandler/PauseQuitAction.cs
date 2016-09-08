public class PauseQuitAction : LevelLoadByNameWithAudioActivation
{
    public override void PerformAction<T> (T triggerInstance)
    {
        // TODO: Call steam manager reset method
        DataCollector.instance.Reset();
        base.PerformAction<T>(triggerInstance);
    }
}