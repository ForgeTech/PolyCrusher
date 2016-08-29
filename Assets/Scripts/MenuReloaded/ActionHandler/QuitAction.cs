using UnityEngine;

public class QuitAction : AbstractActionHandler
{
    public override void PerformAction<T>(T triggerInstance)
    {
        Application.Quit();
    }
}