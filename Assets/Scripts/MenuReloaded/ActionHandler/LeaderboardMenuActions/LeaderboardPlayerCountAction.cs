using UnityEngine;
using System.Collections;
using System;

public enum LeaderboardPlayerCount
{
    AllPlayer = 0,
    OnePlayer = 1,
    TwoPlayer = 2,
    ThreePlayer = 3,
    FourPlayer = 4
}

public class LeaderboardPlayerCountAction : AbstractActionHandler
{
    [SerializeField]
    public LeaderboardPlayerCount playerCount = LeaderboardPlayerCount.AllPlayer;

    public override void PerformAction<T>(T triggerInstance)
    {
        OnActionPerformed();
    }
}
