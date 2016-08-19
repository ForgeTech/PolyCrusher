using UnityEngine;

public class ResolutionAction : AbstractActionHandler
{
    [SerializeField]
    public int width = 1920;

    [SerializeField]
    public int height = 1080;

    public override void PerformAction<T>(T triggerInstance)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        OnActionPerformed();
    }
}