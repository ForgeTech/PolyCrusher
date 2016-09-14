using System;
using UnityEngine;

public enum QualityLevel
{
    Fantastic = 0,  // Max
    Beautiful = 1,  // High
    Good = 2,       // Medium
    Fastest = 3     // Low
}
public class QualityLevelAction : AbstractActionHandler
{
    [SerializeField]
    public QualityLevel qualityLevel = QualityLevel.Fantastic;

    public override void PerformAction<T>(T triggerInstance)
    {
        string[] qualityNames = QualitySettings.names;

        for (int i = 0; i < qualityNames.Length; i++)
        {
            if (qualityNames[i].Equals(qualityLevel.ToString()))
            {
                QualitySettings.SetQualityLevel(i);
                OnActionPerformed();
                break;
            }
        }
    }
}
