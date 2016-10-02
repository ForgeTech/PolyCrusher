using UnityEngine;

/// <summary>
/// The only purpose of this script is to start the DataCollector with the given settings.
/// </summary>
public class DataCollectorStarter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private DataCollectorSettings settings;

	private void Start ()
    {
        if(FindObjectOfType<DataCollector>() == null)
            DataCollector.Initialize(settings);
	}
}