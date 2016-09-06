using UnityEngine;

public enum LeaderboardWorld
{
    AllWorlds = 0,
    Skyrena = 1,
    BurningMan = 2,
    GreenMamba = 3,
    TrickOrTreat = 4,
    IceAndSpice = 5,
    SandMan = 6,
    AtomicPond = 7
}

public class LeaderboardWorldAction : AbstractActionHandler
{
    [SerializeField]
    public LeaderboardWorld world = LeaderboardWorld.AllWorlds;

    public override void PerformAction<T>(T triggerInstance)
    {
        OnActionPerformed();
    }
}
