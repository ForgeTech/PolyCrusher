using UnityEngine;
using System.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ErrorLogger : MonoBehaviour {

    // singleton instance
    private static ErrorLogger _instance;
    public static ErrorLogger Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("_ErrorLogger").AddComponent<ErrorLogger>();
            return _instance;
        }
    }

    private int uploads = 0;
    private string lastLog = "";
    private string lastStackTrace = "";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ErrorLogger]");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if(type == LogType.Error || type == LogType.Exception)
        {
            if (!Application.isEditor && DataCollector.instance.Online && uploads < 5)
            {
                if(!(lastStackTrace.Equals(stackTrace) && lastLog.Equals(logString)))
                {
                    Log log = new Log(logString, stackTrace, type);
                    StartCoroutine(UploadLog(log));
                    uploads++;
                    lastLog = logString;
                    lastStackTrace = stackTrace;
                }
            }
        }
    }

    /// <summary>
    /// sends session via HTTP
    /// </summary>
    IEnumerator UploadLog(Log log)
    {
        string serializedLog = log.ToJson();
        //Debug.Log("[ErrorLogger] serialized: " + serializedLog);

        WWWForm form = new WWWForm();
        form.AddField("data", serializedLog);
        WWW www = new WWW(DataCollector.instance.scriptsAddress + "postLog.php", form);
        yield return www;
        if (www.error == null)
        {
            string response = www.text;
            Debug.Log("[ErrorLogger] WWW Ok: " + response); 
        }
        else
        {
            Debug.LogWarning("[ErrorLogger] WWW Error: " + www.error);
        }
    }
    
    public class Log{
        public Log(string logString, string stackTrace, LogType logtype)
        {
            log = logString;
            stacktrace = stackTrace;
            type = logtype.ToString();
            session_id = DataCollector.instance.GetSessionId();

            graphicsDeviceName = SystemInfo.graphicsDeviceName;
            graphicsMemorySize = SystemInfo.graphicsMemorySize;
            systemMemorySize = SystemInfo.systemMemorySize;
            operatingSystem = SystemInfo.operatingSystem;
            deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
        }

        public string session_id { get; set; }
        public string log { get; set; }
        public string stacktrace { get; set; }
        public string type;
        public string operatingSystem;
        public string graphicsDeviceName;
        public int graphicsMemorySize;
        public int systemMemorySize;
        public string deviceUniqueIdentifier;
    }
}
