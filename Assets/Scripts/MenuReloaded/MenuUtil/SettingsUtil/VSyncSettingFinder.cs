using UnityEngine;

public class VSyncSettingFinder : AbstractSettingFinder
{
    public override int CalculateSelectionIndex()
    {
        if (QualitySettings.vSyncCount >= (int) SwitchEnum.On)
            return 0;
        else 
            return 1;
    }
}