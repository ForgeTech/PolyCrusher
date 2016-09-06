using System;
using UnityEngine;

public class LeaderboardApplyAction : AbstractActionHandler
{
    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;

    private void Start ()
    {
        menuManager = transform.parent.gameObject.GetComponent<AbstractMenuManager>();
        selector = (SelectorWithSubSelector)menuManager.Selector;
    }

    public override void PerformAction<T> (T triggerInstance)
    {
        InvokeSteamRequest();
        OnActionPerformed();
    }

    public void InvokeSteamRequest ()
    {
        GetInvocationData();
    }

    public void GetInvocationData()
    {
        // Iterate through all selector components (Levels, Character count)
        foreach (var pair in selector.SubSelectionEntries)
        {
            // The apply button has no components
            if (pair.Value.Components.Count > 0)
            {
                GameObject selectedMenuComponent = pair.Value.Components[pair.Value.Current];
                AbstractActionHandler componentAction = selectedMenuComponent.GetComponent<AbstractActionHandler>();

                // Apply the action of each menu component
                componentAction.PerformAction<AbstractActionHandler>(this);
            }
        }
    }
}
