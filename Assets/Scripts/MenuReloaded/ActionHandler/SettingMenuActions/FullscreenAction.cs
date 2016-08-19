using UnityEngine;

public class FullscreenAction : AbstractActionHandler
{
    [SerializeField]
    public SwitchEnum isFullScreen = SwitchEnum.On;

    public override void PerformAction<T>(T triggerInstance)
    {
        if (isFullScreen == SwitchEnum.On)
            Screen.fullScreen = true;
        else
            Screen.fullScreen = false;

        OnActionPerformed();
    }
}