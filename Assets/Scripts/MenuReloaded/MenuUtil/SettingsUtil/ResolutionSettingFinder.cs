using UnityEngine;

public class ResolutionSettingFinder : AbstractSettingFinder
{
    public override int CalculateSelectionIndex()
    {
        int width = Screen.width;

        if (width >= 1920)
            return 0;
        else if (width >= 1600)
            return 1;
        else if (width >= 1366)
            return 2;
        else
            return 3;
    }
}
