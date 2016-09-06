using UnityEngine;

public enum LeaderboardPlayerCount
{
    OnePlayer = 1,
    TwoPlayer = 2,
    ThreePlayer = 3,
    FourPlayer = 4
}

public class LeaderboardPlayerCountAction : AbstractActionHandler
{
    [SerializeField]
    public LeaderboardPlayerCount playerCount = LeaderboardPlayerCount.OnePlayer;

    public override void PerformAction<T>(T triggerInstance)
    {
        OnActionPerformed();
    }
}