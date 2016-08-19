using System;
using System.Collections.Generic;
using UnityEngine;

public class ApplySettingsAction : AbstractActionHandler
{
    [SerializeField]
    [Tooltip("The level which should be loaded after Apply was pressed.")]
    private string levelAfterApply;

    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;

    protected void Start()
    {
        menuManager = transform.parent.gameObject.GetComponent<AbstractMenuManager>();
        selector = (SelectorWithSubSelector) menuManager.Selector;
    }

    public override void PerformAction<T>(T triggerInstance)
    {
        // Do quality setting stuff
        ApplyQualitySettings();

        OnActionPerformed();
        Application.LoadLevel(levelAfterApply);
    }

    private void ApplyQualitySettings()
    {
        // Iterate through all selector components (Resolution, Quality, Anti-Aliasing, ...)
        foreach (var pair in selector.SubSelectionEntries)
        {
            // The apply button has no components
            if (pair.Value.Components.Count > 0)
            {
                GameObject selectedSetting = pair.Value.Components[pair.Value.Current];
                AbstractActionHandler settingAction = selectedSetting.GetComponent<AbstractActionHandler>();

                // Apply the action of each selected setting (of the specific component)
                settingAction.PerformAction<AbstractActionHandler>(this);
            }
        }
    }
}