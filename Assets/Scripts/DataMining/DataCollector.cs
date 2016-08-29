using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using System;

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
    [Header("HTTP")]
        [Tooltip("Address where postEvents.php and postSessions.php are located.")]
        public string scriptsAddress = "http://hal9000.schedar.uberspace.de/scripts/";

    [Header("Settings")]
        [Tooltip("Determines how many events should be uploaded at once.")]
        public int bundleSize = 10;
        public bool log = false;
        [Tooltip("Check if all registered events shall be logged in the console.")]
        public bool logEvents = false;

    // VERSION NUMBER
    internal string buildVersion = "0.3";

    // general fields
    private bool sessionRunning = false;
    private Queue eventQueue; // is not generic because then there would be no clone function
    private Session currentSession;

    // list of all events of all local sessions
    //private List<Event> localEvents;

    // for tracking
    private IDictionary<string, int> kills; 
    private IDictionary<string, int> deathtime;

    // leaderboard score
    /// <summary>
    /// Gets or sets the score
    /// </summary>
    public int Score
    {
        get
        {
            return this.score;
        }
        private set
        {
            this.score = value;
            Debug.Log("score:"+score);
        }
    }
    [SerializeField]
    private int score;
    public int intermediateScore; // score to be viewed in the UI

    private ScoreContainer scoreContainer;

    /// <summary>
    ///  for assigning callback, called when downloaded rank is received
    /// </summary>
    public static event RankReceivedDelegate RankReceived;
    public delegate void RankReceivedDelegate(int rank);
    /// <summary>
    /// for assigning callback, called when new event has been registered
    /// </summary>
    public static event EventRegisteredDelegate EventRegistered;
    public delegate void EventRegisteredDelegate(Event e);
    
    // singleton instance
    private static DataCollector _instance;
    public static DataCollector instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("_DataCollector").AddComponent<DataCollector>();
            return _instance;
        }
    }

    public static DataCollectorSettings settings;
    
    /// <summary>
    /// Return character name of player with the most kills
    /// </summary>
    public string playerWithMostKills()
    {
        string character = ReturnStandardValueInCurrentLanguage();
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
        string character = ReturnStandardValueInCurrentLanguage();
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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        eventQueue = new Queue();
        kills = new Dictionary<string, int>();
        deathtime = new Dictionary<string, int>();
        scoreContainer = new ScoreContainer();

        // set settings
        if(settings != null){
            buildVersion = settings.buildVersion;
            log = settings.log;
            logEvents = settings.logEvents;
            scriptsAddress = settings.scriptsAddress;
            bundleSize = settings.bundleSize;
        }
        else
            Debug.Log("[DataCollector] no settings loaded, using standard settings");
    }

    /// <summary>
    /// To be called once at game start!
    /// </summary>
    public static void Initialize(DataCollectorSettings settings)
    {
        Debug.Log("[DataCollector] initializing");

        if (_instance != null)
        {
            Debug.LogError("[DataCollector] already instantiated before initialization");
        }
        DataCollector.settings = settings;
        instance.buildVersion = settings.buildVersion;
    }

    /// <summary>
    /// Initializes connection to database
    /// </summary>
    void Start () {
        EventRegistered += calculateScore;
        DataCollector.EventRegistered += BaseSteamManager.Instance.LogAchievementEvent;
        
        if (enabled)
        {
            // TODO check if server is reachable
        }
	}


    /// <summary>
    /// creates a new session, notifies server and retrieves session id
    /// * should be called at the beginning of game session (before level starts)
    /// </summary>
    public void startSession(GameMode mode)
    {
        if (enabled)
        {
            // if a session is still running, end it
            if (sessionRunning && currentSession != null){
                //endSession();
            }

            // create new session
            currentSession = new Session(mode);
            currentSession.steamId = SteamManager.Instance.GetSteamID();
            currentSession.steamName = SteamManager.Instance.GetSteamName();
            sessionRunning = true;

            switch (GameManager.GameManagerInstance.CurrentGameMode)
            {
                case GameMode.NormalMode:
                    currentSession.mode = "normal";
                    break;
                case GameMode.YOLOMode:
                    currentSession.mode = "yolo";
                    break;
            }

            // upload session
            StartCoroutine(UploadSession());

            GameManager.WaveStarted += () => { new Event(Event.TYPE.waveUp).addWave().send(); };
            PlayerManager.AllPlayersDeadEventHandler += () => { endSession("Heinzi"); };

            // reset score
            score = 0;
            intermediateScore = 0;
            scoreContainer = new ScoreContainer();
        }
    }

    public void startSession()
    {
        startSession(GameMode.NormalMode);
    }

    /// <summary>
    /// To be called if current game session ends, with name and email for highscore
    /// </summary>
    public void endSession(string gameName)
    {
        if (enabled && sessionRunning)
        {
            Event endEvent = new Event(Event.TYPE.sessionEnd);
            endEvent.addPlayerCount().addWave().addLevel();
            endEvent.addGameName(gameName);
            endEvent.addPlayerCharacters();
            endEvent.addMode(currentSession.mode);
            addEvent(endEvent);
        }

        sessionRunning = false;

        // is the hound burried here?
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

        OnEventRegistered(e);
    }

    /// <summary>
    /// Add event to send queue and upload when big enough
    /// </summary>
    private void addToSendQueue(Event e)
    {
        // reference current session
        e.session_id = currentSession._id;

        // set event time (if session end take official time)
        if(e.type == Event.TYPE.sessionEnd)
        {
            e.time = PlayerManager.PlayTime.TotalTime;
        }
        else
        {
            e.time = (int)(Time.time * 1000) - currentSession.time;
        }

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
            Debug.Log("[DataCollector] " + e.ToString());
        }

        // if event queue gets big enough upload data
        if (enabled) { 
            if (eventQueue.Count >= bundleSize || e.type == Event.TYPE.sessionEnd)
            {
                    StartCoroutine(UploadEvents());
            }
        }
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

        // encode serialized events
        serializedEvents = encode(serializedEvents);

        if (log) { Debug.Log("[DataCollector] uploading " + e.Length + " events"); }

        WWWForm form = new WWWForm();
        form.AddField("data", serializedEvents);
        WWW www = new WWW(scriptsAddress + "postEvents.php", form);
        yield return www;
        if (www.error == null)
        {
            string response = www.text;
            if (response == "success")
            {
                if (log) { Debug.Log("[DataCollector] event upload successful"); }
            }
            else
            {
                if (log) { Debug.Log("[DataCollector] unexpected response: " + response); }
                for (int i = 0; i < e.Length; i++)
                {
                    eventQueue.Enqueue(e[i]);  // reinsert
                }
            }
        }
        else
        {
            for (int i = 0; i < e.Length; i++)
            {
                eventQueue.Enqueue(e[i]);  // reinsert
            }
            //if (log) 
            Debug.Log("[DataCollector] WWW Error: " + www.error);
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
                if (log) { Debug.Log("[DataCollector] WWW Ok: " + response); }
            }
            else
            {
                //if (log) 
                Debug.Log("[DataCollector] WWW Ok, Unexpected response:" + response);
            }
        }else{
            //if (log)
            Debug.Log("[DataCollector] WWW Error: " + www.error);
        }
    }
    
    public IEnumerator DownlaodHighscoreRank()
    {
        WWWForm form = new WWWForm();

        String data;
        if (currentSession.mode != "yolo")
        {
            data = Event.getWave().ToString();
        }
        else
        {
            data = PlayerManager.PlayTime.TotalTime.ToString();
        }

        form.AddField("data", data);
        form.AddField("mode", currentSession.mode);

        WWW www = new WWW(scriptsAddress + "getRank.php", form);
        yield return www;
        int rank = 0;

        if (www.error == null)
        {
            string response = www.text;
            
            try
            {
                rank = Convert.ToInt32(response);
                if (log) { 
                    Debug.Log("[DataCollector] WWW Ok: " + response);
                }
            }
            catch (FormatException e)
            {
                //if (log) { 
                Debug.Log("[DataCollector] WWW Ok, Unexpected response:" + response);
                Debug.Log(e.ToString());
                //}
            }
        }
        else
        {
            //if (log) {
                Debug.Log("[DataCollector] WWW Error: " + www.error);
            //}
        }

        OnRankReceived(rank);
    }


    public void OnRankReceived(int rank)
    {
        if (RankReceived != null)
        {
            RankReceived(rank);
        }

        // Watch out! also event registered is reset
        ResetDelegates();
    }

    public void OnEventRegistered(Event e)
    {
        if (EventRegistered != null)
        {
            EventRegistered(e);
        }
    }


    /// <summary>
    /// Resets all delegates
    /// </summary>
    protected void ResetDelegates()
    {
        RankReceived = null;
        EventRegistered = null;
    }

    public static string encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    /*
    /// <summary>
    /// Loads all locally saved events (main purpose: local highscore)
    /// </summary>
    public void LoadEvents()
    {
        // mark events as saved!
        // TODO
    }

    /// <summary>
    /// Saves all new events locally
    /// </summary>
    public void SaveEvents()
    {
       // iterate through all not saved events
       
       foreach (Event e in localEvents.Where(e => (e.isSaved == false)))
       {
       }
       // TODO
    }
    */

    /// <summary>
    /// returns a "no one" string in the current active language
    /// </summary>
    private string ReturnStandardValueInCurrentLanguage()
    {
        string currentLanguage = PlayerPrefs.GetString("SelectedLanguage");
        if (currentLanguage != null)
        {
            switch (currentLanguage)
            {
                case "German": return "KEINER"; 
                case "English": return "NO ONE";
            }
        }
        return "NO ONE";
    }


    int playerDeathsInWave = 0;
    int resourceValueBefore = 0;
    private void calculateScore(Event e)
    {
        int scoreBefore = Score;

       

        switch (e.type)
        {
            case Event.TYPE.kill:
                

                if(e.enemy == "B055")
                {
                    scoreContainer.addBossKills(1);
                    Score += 2500;
                    WaveCounterManager.instance.ScorePopup(2500);
                }

                if(e.character == "LineSystem") // || e.character == "laser_trap"
                {
                    Score += 100;
                    scoreContainer.addCutKills(1);
                    WaveCounterManager.instance.ScorePopup(100);
                }

                // another hound burried: this score always hangs back one kill, because the kill event is triggered before the AccumulatedRessourceValue can be updated
                int interpolationAddition = (int)((GameManager.gameManagerInstance.AccumulatedRessourceValue - resourceValueBefore) / (float)GameManager.gameManagerInstance.EnemyRessourcePool * 10000);
                //Debug.Log("[Score]" + interpolationAddition);
                intermediateScore += interpolationAddition;

                resourceValueBefore = GameManager.GameManagerInstance.AccumulatedRessourceValue;
                break;

            case Event.TYPE.superAbility:
                int n = 0;

                if(e.kills != null)
                {
                    n += (int)e.kills * 100;
                    scoreContainer.addPolyKills((int)e.kills);
                }
                scoreContainer.addPolysTriggered(1);
                Score += 1000 + n;
                WaveCounterManager.instance.ScorePopup(1000 + n);
                break;

            case Event.TYPE.death:
                playerDeathsInWave++;
                break;

            case Event.TYPE.powerup:
                Score += 100;
                scoreContainer.addPowerupsCollected(1);
                WaveCounterManager.instance.ScorePopup(100);
                break;

            case Event.TYPE.waveUp:
                if(e.wave != 1)
                {
                    if (playerDeathsInWave != 0)
                    {
                        Score -= 1000 * playerDeathsInWave;
                        scoreContainer.addPlayerRevials(playerDeathsInWave);
                        WaveCounterManager.instance.ScorePopup(-1000*playerDeathsInWave);
                    }

                    Score += 10000;
                    WaveCounterManager.instance.ScorePopup(10000);
                }
                //intermediateScore -= entireInterpolationAddition;
                scoreBefore = Score;
                intermediateScore = Score;
                resourceValueBefore = 0;

                playerDeathsInWave = 0;
                break;
            case Event.TYPE.sessionEnd:
                float wave = Event.getWave();
                int s = (int)((wave - (int)wave) * 10000);
                Score += s;

                scoreContainer.setWave(wave);

                break;
        }
        //Debug.Log("[Score]" + (Score - scoreBefore));
        intermediateScore += Score - scoreBefore;
    }
    
    public ScoreContainer getScoreContainer()
    {
        return scoreContainer;
    }
}

/*
  ////// EXAMPLE EVENT ////////
  new Event(Event.TYPE.ability).addPos(this.transform).addWave().addLevel().addCharacter(this.playerName).addPlayerCount().send();
*/
