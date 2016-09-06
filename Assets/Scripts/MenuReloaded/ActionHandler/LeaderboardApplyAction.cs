using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardApplyAction : AbstractActionHandler
{
    [SerializeField]
    private LeaderboardHelper leaderboardHelper;

    private AbstractMenuManager menuManager;
    private SelectorWithSubSelector selector;

    private RequestData requestData = null;

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

        if (requestData != null)
        {
            BaseSteamManager.Instance.RequestLeaderboardEntries(requestData.level, requestData.playerCount, 1, 10,
                (List<LeaderboardEntry> entries) => {
                    leaderboardHelper.SetLeaderboardEntries(entries);
            });
        }
    }

    public void GetInvocationData()
    {
        string levelName = null;
        int playerCount = -1;
        requestData = null;

        // Iterate through all selector components (Levels, Character count)
        foreach (var pair in selector.SubSelectionEntries)
        {
            // The apply button has no components
            if (pair.Value.Components.Count > 0)
            {
                GameObject selectedMenuComponent = pair.Value.Components[pair.Value.Current];
                AbstractActionHandler componentAction = selectedMenuComponent.GetComponent<AbstractActionHandler>();

                if (componentAction is LeaderboardWorldAction)
                    levelName = ((LeaderboardWorldAction)componentAction).world.ToString();
                else if (componentAction is LeaderboardPlayerCountAction)
                    playerCount = (int)((LeaderboardPlayerCountAction)componentAction).playerCount;

                    // Apply the action of each menu component
                    componentAction.PerformAction<AbstractActionHandler>(this);
            }
        }

        requestData = new RequestData(levelName, playerCount);
    }

    private class RequestData
    {
        internal readonly string level;
        internal readonly int playerCount;

        public RequestData(string level, int playerCount)
        {
            this.level = level;
            this.playerCount = playerCount;
        }
    }
}
