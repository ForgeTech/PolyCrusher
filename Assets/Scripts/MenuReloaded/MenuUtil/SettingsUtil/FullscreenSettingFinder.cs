using UnityEngine;

public class FullscreenSettingFinder : AbstractSettingFinder
{
    public override int CalculateSelectionIndex()
    {
        if (Screen.fullScreen)
            return 0;
        else
            return 1;
    }
}