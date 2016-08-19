using System;
using UnityEngine;

public enum QualityLevel
{
    Fantastic,  // Max
    Beautiful,  // High
    Good,       // Medium
    Fastest     // Low
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
            if (qualityNames[i].Equals(qualityNames.ToString()))
            {
                QualitySettings.SetQualityLevel(i);
                OnActionPerformed();
                break;
            }
        }
    }
}
