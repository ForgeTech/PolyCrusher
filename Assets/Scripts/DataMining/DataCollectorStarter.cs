using UnityEngine;

/// <summary>
/// The only purpose of this script is to start the DataCollector with the given settings.
/// </summary>
public class DataCollectorStarter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private DataCollectorSettings settings;

    private static bool commandLineEventBuild = false;

	private void Start ()
    {
        // Set event build based on command line Flag
        if (commandLineEventBuild && settings != null)
            settings.eventBuild = true;

        if(FindObjectOfType<DataCollector>() == null)
            DataCollector.Initialize(settings);
	}

    /// <summary>
    /// Static method which should be called per command line argument
    /// Example: -executeMethod DataCollectorStarter.StartEventBuild
    /// </summary>
    static void StartEventBuild()
    {
        Debug.Log("Event Build Activated per command line argument.");
        commandLineEventBuild = true;
    }
}