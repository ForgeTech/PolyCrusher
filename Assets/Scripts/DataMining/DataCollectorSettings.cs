using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Hint", menuName = "ScriptableObject/DataCollectorSetting", order = 2)]
public class DataCollectorSettings : ScriptableObject {
    [Header("HTTP")]
    [Tooltip("Address where postEvents.php and postSessions.php are located.")]
    public string scriptsAddress = "http://hal9000.schedar.uberspace.de/scripts/";
    internal string buildVersion = "0.3";

    [Header("Settings")]
    [Tooltip("Determines how many events should be uploaded at once.")]
    public int bundleSize = 10;
    public bool log = false;
    [Tooltip("Check if all registered events shall be logged in the console.")]
    public bool logEvents = false;

    public bool eventBuild = true;
}
