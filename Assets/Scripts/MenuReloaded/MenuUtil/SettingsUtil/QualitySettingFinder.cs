using UnityEngine;
using System.Collections.Generic;
using System;

public class QualitySettingFinder : AbstractSettingFinder
{
    public override int CalculateSelectionIndex()
    {
        int quality = QualitySettings.GetQualityLevel();
        string[] qualitySettings = QualitySettings.names;

        if(qualitySettings[quality].Equals(QualityLevel.Fantastic.ToString()))
            return (int)QualityLevel.Fantastic;
        else if (qualitySettings[quality].Equals(QualityLevel.Beautiful.ToString()))
            return (int)QualityLevel.Beautiful;
        else if (qualitySettings[quality].Equals(QualityLevel.Good.ToString()))
            return (int)QualityLevel.Good;
        else if (qualitySettings[quality].Equals(QualityLevel.Fastest.ToString()))
            return (int)QualityLevel.Fastest;

        return (int) QualityLevel.Fastest;
    }
}
