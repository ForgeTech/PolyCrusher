using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System;

public delegate void RankReceivedDelegate(int rank);

/// <summary>
/// This class uploads game meta data to our mongoDB server.
/// You can send events and start sessions which will be converted to mongoDB documents.
/// Start a new session when a new game is started.
/// When you add a event and no session is started, a new session gets started automatically
/// (not recommended due incorrect session start time).
/// Events (which reference the current session) are sent while the session is running.
/// </summary>
public class DataCollector : MonoBehaviour
{
    public enum ConnectionType { HTTP = 0, MongoDB = 1 }
    [Header("Database Connection")]
        public ConnectionType connectVia = ConnectionType.HTTP;

    [Header("HTTP")]
        [Tooltip("Address where postEvents.php and postSessions.php are located.")]
        public string scriptsAddress = "http://hal9000.schedar.uberspace.de/scripts/";

    [Header("MongoDB Driver")]
        public string serverIP = "185.26.156.41:61116";
        public bool authenticate = false;

    [Header("Settings")]
        [Tooltip("Determines how many events should be uploaded at once.")]
        public int bundleSize = 10;
        [Tooltip("Version number which will be saved in the session objects.")]
        public string buildVersion = "0.1b";
        public bool log = true;
        [Tooltip("Check if all registered events shall be logged in the console.")]
        public bool logEvents = false;
        
        

    // MongoDB fields
    private MongoServer server;
    private MongoCollection<Event> events;
    private MongoCollection<Session> sessions;
    private string databaseName = "polycrusher";
    private string user = "polynaut";
    private string password = "crushthempolys";

    // HTTP fields
    // ---

    // general fields
    private bool sessionRunning = false;
    private Queue eventQueue;
    private Session currentSession;

    // for tracking
    private IDictionary<string, int> kills; 
    private IDictionary<string, int> deathtime;

    public static event RankReceivedDelegate RankReceived;

    // singleton instance
    private static DataCollector _instance;
    public static DataCollector instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<DataCollector>();
            return _instance;
        }
    }

    /// <summary>
    /// Return character name of player with the most kills
    /// </summary>
    public string playerWithMostKills()
    {
        string character = "No one";
        int topKills = 0;
        foreach(KeyValuePair<string,int> kv in kills)
        {
            if (kv.Value > topKills)
            {
                character = kv.Key;
                topKills = kv.Value;
            }
        }
        return character;
    }

    /// <summary>
    /// Return character name of player with least deaths
    /// </summary>
    public string playerWithLeastDeathtime()
    {
        string character = "No one";
        int bottomDeathtime = -1;
        foreach (KeyValuePair<string, int> kv in deathtime)
        {
            if (kv.Value < bottomDeathtime || bottomDeathtime == -1)
            {
                character = kv.Key;
                bottomDeathtime = kv.Value;
            }
        }
        return character;
    }

    public class Session
    {
        public Session()
        {
            macAddress = getMAC();
            version = DataCollector.instance.buildVersion;
            inEditor = Application.isEditor; 
            time = (int)(Time.time * 1000);
        }

        //public ObjectId _id { get; set; }
        [BsonIgnore]
        public string _id { get; set; }
        public string macAddress { get; set; }
		public string version { get; set; }
        public bool inEditor { get; set; }

        //public DateTime Timestamp { get; set; }

        /// <summary>
        /// start time of session
        /// </summary>
        [BsonIgnore]
        public int time { get; set; }
    }


	/// <summary>
	/// Initializes connection to database
	/// </summary>
	void Start () {
        eventQueue = new Queue();
        kills = new Dictionary<string, int>();
        deathtime = new Dictionary<string, int>();

        if (DataCollector.instance.enabled)
        {
            switch (connectVia)
            {
                // CONNECT VIA HTTP
                case ConnectionType.HTTP:

                    // TODO  Check if server is reachable

                    break;

                // CONNECT VIA MONGO DB DRIVER
                case ConnectionType.MongoDB:
                    string url = "mongodb://";

                    if (authenticate)
                    {
                        url += user + ":" + password + "@";
                    }

                    url += serverIP;

                    MongoClient client = new MongoClient(url);
                    server = client.GetServer();

                    // check if server can be reached
                    try {
                        server.Ping();
                    } catch {
                        server = null;
                    }

                    if (server!= null)
                    {
                        server.Connect();
                        MongoDatabase db = server.GetDatabase(databaseName);
                        events = db.GetCollection<Event>("events");
                        sessions = db.GetCollection<Session>("sessions");
                        if (log) { Debug.Log("DataCollector: Connected to database."); }
                    }
                    else
                    {
                        // deactivate DataCollector if connection fails
                        if (log) { Debug.Log("DataCollector: Could not connect to database."); }
                        DataCollector.instance.enabled = false;
                    }
                    break;
            }


            startSession();


        }
	}


    /// <summary>
    /// creates a new session, notifies server and retrieves session id
    /// * should be called at the beginning of game session (before level starts)
    /// </summary>
    public void startSession(){
        if (DataCollector.instance.enabled)
        {
            // if a session is still running, end it
            if (sessionRunning && currentSession != null){
                //endSession();
            }

            // create new session
            currentSession = new Session();
            sessionRunning = true;

            // upload session
            switch (connectVia)
            { 
                case ConnectionType.HTTP:
                    StartCoroutine(UploadSession());
                    break;
                case ConnectionType.MongoDB:
                    Thread uploadThread = new Thread(delegate()
                    {
                        sessions.Insert(currentSession);

                        // check if session id could be retrieved (needed for referencing session in events)
                        if (currentSession._id == null)
                        {
                            if (log) { Debug.Log("Could not retrieve session_id."); }
                            currentSession = null;
                            sessionRunning = false;
                        }
                    });

                    uploadThread.Start();
                    break;
            }
        }
    }

    /// <summary>
    /// To be called if current game session ends, with name and email for highscore
    /// </summary>
    public void endSession(string gameName)
    {
        if (DataCollector.instance.enabled && sessionRunning)
        {
            Event endEvent = new Event(Event.TYPE.sessionEnd);
            endEvent.addPlayerCount().addWave().addLevel();
            endEvent.addGameName(gameName);
            addEvent(endEvent);
        }

        sessionRunning = false;
        eventQueue.Clear();
        kills.Clear();
        deathtime.Clear();
    }

    /// <summary>
    /// add event to send queue
    /// bevore adding an event session must run
    /// </summary>
    public void addEvent(Event e)
    {
        if (!sessionRunning || currentSession == null)
        {
            startSession();
            addToSendQueue(e);
        }
        else
        {
            addToSendQueue(e);
        }
    }

    /// <summary>
    /// Add event to send queue and upload when big enough
    /// </summary>
    private void addToSendQueue(Event e)
    {
        // reference current session
        e.session_id = currentSession._id;

        // set event time
        e.time = (int)(Time.time * 1000) - DataCollector.instance.currentSession.time;

        // add event to queue for later upload
        eventQueue.Enqueue(e);

        // track kills locally
        if (e.type == Event.TYPE.kill)
        {
            if(kills.ContainsKey(e.character)){
                kills[e.character] = kills[e.character]+1;
            }else{
                kills.Add(e.character, 1);
            }
        }

        // track death time
        // TODO
        if (e.type == Event.TYPE.death)
        {
            if (deathtime.ContainsKey(e.character))
            {
                deathtime[e.character] = deathtime[e.character] + 1;
            }
            else
            {
                deathtime.Add(e.character, 1);
            }
        }

        // track deaths locally
        if (e.type == Event.TYPE.death)
        {
            if (kills.ContainsKey(e.character))
            {
                kills[e.character] = kills[e.character] + 1;
            }
            else
            {
                kills.Add(e.character, 1);
            }
        }

        // event log
        if (logEvents)
        {
            Debug.Log("DataCollector: Added " + e.ToString());
        }

        // if event queue gets big enough upload data
        if (DataCollector.instance.enabled) { 
        if (eventQueue.Count >= bundleSize || e.type == Event.TYPE.sessionEnd)
        {
                switch (connectVia)
                {
                    // upload via HTTP
                    case ConnectionType.HTTP:
                        StartCoroutine(UploadEvents());
                        break;
                    // upload via MongoDB
                    case ConnectionType.MongoDB:

                        Thread uploadThread = new Thread(delegate()
                        {
                            bool connected = true;

                            try {
                                server.Ping();
                            } catch {
                                connected = false;
                                if (log) { Debug.Log("DataCollector: No connection. Retry with next event."); }
                            }

                            if (connected)
                            {
                                IEnumerable list = (IEnumerable)eventQueue.Clone();
                                events.InsertBatch(typeof(Event), list);

                                eventQueue.Clear(); // thread save?
                            }
                        });

                        uploadThread.Start();
                        break;
                }
            

            
        }
        }
    }


    /// <summary>
    /// retrieves mac address (get network interfaces)
    /// </summary>
    private static string getMAC()
    {
        //IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string info = "";

        // only save address of first adapter
        for (int n = 0; n< 1; n++)
        {
            if (n > 0)
            {
                 info += "\n";
            }


            PhysicalAddress address = nics[n].GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            string mac = null;
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + "-");
                }
            }

            info += mac;
        }
        return info;
    }


    /// <summary>
    /// sends events via HTTP
    /// </summary>
    IEnumerator UploadEvents()
    {
        // move all events to temporary array to reinsert data is upload fails
        Event[] e = new Event[eventQueue.Count];
        eventQueue.CopyTo(e,0);
        eventQueue.Clear();
        string serializedEvents = e.ToJson();

        if (log) { Debug.Log("DataCollector: uploading " + e.Length + " events"); }

        WWWForm form = new WWWForm();
        form.AddField("data", serializedEvents);
        WWW www = new WWW(scriptsAddress + "postEvents.php", form);
        yield return www;
        if (www.error == null)
        {
            string response = www.text;
            if (response == "success")
            {
                if (log) { Debug.Log("DataCollector: event upload successful"); }
            }
            else
            {
                if (log) { Debug.Log("DataCollector: unexpected response: " + response); }
                for (int i = 0; i < e.Length; i++)
                {
                    eventQueue.Enqueue(e[i]);  // reinsert
                }
            }
        }
        else
        {
            if (log) { Debug.Log("WWW Error: " + www.error); }
            for (int i = 0; i < e.Length; i++)
            {
                eventQueue.Enqueue(e[i]);  // reinsert
            }
        }
    }

    /// <summary>
    /// sends session via HTTP
    /// </summary>
    IEnumerator UploadSession()
    {
        string serializedSession = currentSession.ToJson();

        WWWForm form = new WWWForm();
        form.AddField("data", serializedSession);
        WWW www = new WWW(scriptsAddress + "postSession.php", form);
        yield return www;
        if (www.error == null)
        {
            string response = www.text;
            if (response.Length == 24)  // check if response is 24 characters long == id
            {
                currentSession._id = response;
                if (log) { Debug.Log("WWW Ok: " + response); }
            }
            else
            {
                if (log) { Debug.Log("WWW Ok, Unexpected response:" + response); }
            }
        }else{
            if (log) { Debug.Log("WWW Error: " + www.error); }
        }
    }

    public IEnumerator DownlaodHighscoreRank()
    {
        WWWForm form = new WWWForm();
        form.AddField("data", Event.getWave().ToString());
        WWW www = new WWW(scriptsAddress + "getRank.php", form);
        yield return www;
        int rank = 0;

        if (www.error == null)
        {
            string response = www.text;
            
            try
            {
                rank = Convert.ToInt32(response);
                if (log) { Debug.Log("WWW Ok: " + response); }
            }
            catch (FormatException e)
            {
                if (log) { 
                    Debug.Log("WWW Ok, Unexpected response:" + response);
                    Debug.Log(e.ToString());
                }
            }
        }
        else
        {
            if (log) { Debug.Log("WWW Error: " + www.error); }
        }

        OnRankReceived(rank);
        ResetValues();
    }


    public void OnRankReceived(int rank)
    {
        if (RankReceived != null)
        {
            RankReceived(rank);
        }
    }

    public void ResetValues()
    {
        RankReceived = null;
    }
}

/*
  ////// EXAMPLE EVENT ////////
  new Event(Event.TYPE.ability).addPos(this.transform).addWave().addLevel().addCharacter(this.playerName).addPlayerCount().send();
*/
