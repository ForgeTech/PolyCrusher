using UnityEngine;

public class LevelLoadByNameWithPlayerContainer : LevelLoadByName
{
    [SerializeField]
    private string levelNameForPlayerContainer;

    public override void PerformAction<T>(T triggerInstance)
    {
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