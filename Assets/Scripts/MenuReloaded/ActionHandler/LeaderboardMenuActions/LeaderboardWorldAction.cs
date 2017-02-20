using UnityEngine;

public enum LeaderboardWorld
{
    Skyrena = 1,
    BurningMan = 2,
    GreenMamba = 3,
    TrickOrTreat = 4,
    IceAndSpice = 5,
    SandMan = 6,
    AtomicPond = 7,
    NuclearBar = 8
}

public class LeaderboardWorldAction : AbstractActionHandler
{
    [SerializeField]
    public LeaderboardWorld world = LeaderboardWorld.Skyrena;

    public override void PerformAction<T>(T triggerInstance)
    {
        OnActionPerformed();
    }
}
