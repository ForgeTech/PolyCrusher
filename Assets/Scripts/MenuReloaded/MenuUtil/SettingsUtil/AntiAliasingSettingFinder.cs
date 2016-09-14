using UnityEngine;

public class AntiAliasingSettingFinder : AbstractSettingFinder
{
    public override int CalculateSelectionIndex()
    {
        int setting = QualitySettings.antiAliasing;

        if (setting == (int)AntiAliasing.Eight)
            return 0;
        else if (setting == (int)AntiAliasing.Four)
            return 1;
        else if (setting == (int)AntiAliasing.Two)
            return 2;
        else if (setting == (int)AntiAliasing.Off)
            return 3;

        return 3;
    }
}
