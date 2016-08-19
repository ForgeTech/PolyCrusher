using UnityEngine;

public enum AntiAliasing
{
    Eight = 8,
    Four = 4,
    Two = 2,
    Off = 0
}

public class AntiAliasingAction : AbstractActionHandler
{
    [SerializeField]
    public AntiAliasing antiAliasing = AntiAliasing.Eight;

    public override void PerformAction<T>(T triggerInstance)
    {
        QualitySettings.antiAliasing = (int) antiAliasing;
        OnActionPerformed();
    }
}
