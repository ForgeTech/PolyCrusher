using UnityEngine;

public class LevelLoadByNameWithPlayerContainer : LevelLoadByName
{
    [SerializeField]
    private string levelNameForPlayerContainer;

    public override void PerformAction<T>(T triggerInstance)
    {
        // TODO: Implement logic to set the level data in the PlayerSelectionContainer!
        // Maybe give the conainer a tag to find it faster.
        SaveLevelIntoSelectionContainer();
        base.PerformAction<T>(triggerInstance);
    }

    private void SaveLevelIntoSelectionContainer()
    {
        PlayerSelectionContainer container = GameObject.FindGameObjectWithTag("GlobalScripts").GetComponent<PlayerSelectionContainer>();

        if (container != null)
            container.levelName = levelNameForPlayerContainer;
        else
            Debug.LogError("No selection container was found!");
    }
}