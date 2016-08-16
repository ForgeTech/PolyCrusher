using UnityEngine;

public class LevelLoadByNameWithPlayerContainer : LevelLoadByName
{
    [SerializeField]
    private string levelNameForPlayerContainer;

    public override void PerformAction<T>(T triggerInstance)
    {
        // TODO: Implement logic to set the level data in the PlayerSelectionContainer!
        // Maybe give the conainer a tag to find it faster.
        base.PerformAction<T>(triggerInstance);
    }
}