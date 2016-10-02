using UnityEngine;
using System.Collections;
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
        public string serverIP = "185.26.156.41";

    [Header("Settings")]
        [Tooltip("Determines how many events should be uploaded at once.")]
        public int bundleSize = 10000;
        public bool log = false;
        [Tooltip("Check if all registered events shall be logged in the console.")]
        public bool logEvents = false;

    private bool online = true;
    public bool Online
    {
        get { return online; }
    }

    // VERSION NUMBER
    internal string buildVersion = "0.4";
    internal bool eventBuild = true;

    // general fields
    private bool sessionRunning = false;
    private Queue eventQueue; // is not generic because then there would be no clone function
    private Session currentSession;

    // list of all events of all local sessions
    //private List<Event> localEvents;

    // for tracking
   // private IDictionary<string, int> deathtime;

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
        }
    }
    [SerializeField]
    private int score;
    public float intermediateScore; // score to be viewed in the UI

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
    
   

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        eventQueue = new Queue();
        scoreContainer = new ScoreContainer();

        Debug.Log("<color=purple>P</color><color=blue>O</color><color=cyan>L</color><color=green>Y</color><color=yellow>C</color><color=orange>R</color><color=red>U</color><color=purple>S</color><color=blue>H</color><color=cyan>E</color><color=yellow>R</color>");
       
        // set settings
        if (settings != null){
            buildVersion = settings.buildVersion;
            log = settings.log;
            logEvents = settings.logEvents;
            scriptsAddress = settings.scriptsAddress;
            bundleSize = settings.bundleSize;
            eventBuild = settings.eventBuild;
        }
        else
            Debug.Log("[DataCollector] no settings loaded, using standard settings");
    }

    /// <summary>
    /// To be called once at game start!
    /// </summary>
    public static void Initialize(DataCollectorSettings settings)
    {
        //Debug.Log("[DataCollector] initializing");

        if (_instance != null)
        {
            Debug.LogError("[DataCollector] already instantiated before initialization");
        }
        DataCollector.settings = settings;
        instance.buildVersion = settings.buildVersion; // doing random thing calling instance to start instanciation
    }

    /// <summary>
    /// Initializes connection to database
    /// </summary>
    void Start () {
        //EventRegistered += (Event e) => { Invoke(calculateScore(e), 10); };
        //EventRegistered += calculateScore;
        EventRegistered += calculateScore;
        EventRegistered += InvokeDisplayScoreCalcDelayed;
        DataCollector.EventRegistered += BaseSteamManager.Instance.LogAchievementEvent;
        StartCoroutine(TestConnection());

        if (ErrorLogger.Instance.isActiveAndEnabled)
        {
            // error logger enabled
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
            // if a session is still running, warn
            if (sessionRunning){
                //endSession();
                Debug.LogError("[DataCollector] starting new session when there is still one running.");
            }
            
            StartCoroutine(TestConnection());

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
            PlayerManager.AllPlayersDeadEventHandler += () => { endSession(); };

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
    /// To be called if current game session ends
    /// </summary>
    public void endSession()
    {
        if (enabled && sessionRunning)
        {
            Event endEvent = new Event(Event.TYPE.sessionEnd);
            endEvent.addPlayerCount().addWave().addLevel();
            if(!eventBuild && currentSession.steamName != null && !currentSession.steamName.Equals(""))
            {
                endEvent.addGameName(currentSession.steamName);
            }
            else
            {
                endEvent.addGameName(RandomNameGenerator.Generate());
            }

            Debug.Log(RandomNameGenerator.Generate());

            endEvent.addPlayerCharacters();
            endEvent.addMode(currentSession.mode);
            addEvent(endEvent);
        }

        sessionRunning = false;

        // is the hound burried here?
        eventQueue.Clear();
    }

    public void Reset()
    {
        Debug.Log("[DataCollector] Session interrupted.");
        sessionRunning = false;
        eventQueue.Clear();
    }

    public string GetSessionId()
    {
        if(currentSession != null && currentSession._id != null)
        {
            return currentSession._id;
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// add event to send queue
    /// bevore adding an event session must run
    /// </summary>
    public void addEvent(Event e)
    {
        if (e.type != Event.TYPE.join && (!sessionRunning || currentSession == null))
        {
            Debug.LogError("[DataCollector] No session running. Event not logged." + e.ToString());
        }
        else
        {
            if(e.type != Event.TYPE.join)
            {
                // reference current session
                e.session_id = currentSession._id;

                // set event time (if session end take official time)
                if (e.type == Event.TYPE.sessionEnd)
                {
                    e.time = PlayerManager.PlayTime.TotalTime;
                }
                else
                {
                    e.time = (int)(Time.time * 1000) - currentSession.time;
                }
            }

            OnEventRegistered(e);

            // event log
            if (logEvents)
            {
                Debug.Log("[DataCollector] " + e.ToString());
            }

            // add event to queue for later upload
            eventQueue.Enqueue(e);

            // if event queue gets big enough upload data
            if (enabled && online)
            {
                if (eventQueue.Count >= bundleSize || e.type == Event.TYPE.sessionEnd)
                {
                    StartCoroutine(UploadEvents());
                }
            }
        }
    }

    /// <summary>
    /// sends events via HTTP
    /// </summary>
    IEnumerator UploadEvents()
    {
        if (online)
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
                    /*
                    for (int i = 0; i < e.Length; i++)
                    {
                        eventQueue.Enqueue(e[i]);  // reinsert
                    }
                    */
                }
            }
            else
            {
                /*
                for (int i = 0; i < e.Length; i++)
                {
                    eventQueue.Enqueue(e[i]);  // reinsert
                }
                */
                //if (log) 
                Debug.Log("[DataCollector] WWW Error: " + www.error);
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
    
    int resourceValueBefore = 0;
    private void calculateDisplayScore(Event e)
    {
        switch (e.type)
        {
            case Event.TYPE.kill:

                if (e.character != "PolygonSystem")
                {
                    // another hound burried: this score always hangs back one kill, because the kill event is triggered before the AccumulatedRessourceValue can be updated
                    float interpolationAddition = (GameManager.gameManagerInstance.AccumulatedRessourceValue - resourceValueBefore) / (float)GameManager.gameManagerInstance.EnemyRessourcePool * 10000;
                    //intermediateScore += interpolationAddition - 0.00001f;
                   // intermediateScore = Score + (float)GameManager.GameManagerInstance.AccumulatedRessourceValue / (float)GameManager.gameManagerInstance.EnemyRessourcePool;
                    //Debug.Log("<color=red>[DataCollector]</color> popup " + interpolationAddition + " wave " + e.wave);
                    WaveCounterManager.instance.ScorePopup((int)interpolationAddition);

                    resourceValueBefore = GameManager.GameManagerInstance.AccumulatedRessourceValue;
                }
                
                break;
            case Event.TYPE.superAbility:
                int n = 0;
                if (e.kills != null)
                {
                    n += (int)e.kills * 100;
                }
                float scorevalueOfKilledEnemys = ((GameManager.gameManagerInstance.AccumulatedRessourceValue - resourceValueBefore) / (float)GameManager.gameManagerInstance.EnemyRessourcePool * 10000);
                float scoreGain = 1000 + n + scorevalueOfKilledEnemys;
                WaveCounterManager.instance.ScorePopup((int)scoreGain);
                //intermediateScore += scoreGain;
                resourceValueBefore = GameManager.GameManagerInstance.AccumulatedRessourceValue;
                break;
            case Event.TYPE.waveUp:
                //intermediateScore = Score;
                resourceValueBefore = 0;
                break;
            case Event.TYPE.sessionEnd:
                resourceValueBefore = 0; 
                break;
        }

        intermediateScore = Score + (float)GameManager.GameManagerInstance.AccumulatedRessourceValue / (float)GameManager.gameManagerInstance.EnemyRessourcePool * 10000;
        //Debug.Log("inter:" + intermediateScore + " score:" + Score);
    }

    int playerDeathsInWave = 0;
    
    private void calculateScore(Event e)
    {
        //int scoreBefore = Score;

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
                //WaveCounterManager.instance.ScorePopup(1000 + n);
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
                }
                //resourceValueBefore = 0;
                //intermediateScore -= entireInterpolationAddition;
                //scoreBefore = Score;
                intermediateScore = Score;
                playerDeathsInWave = 0;
                break;
            case Event.TYPE.sessionEnd:
                float wave = Event.getWave();
                scoreContainer.setWave(wave);
                if(e.playerCount != null)
                {
                    scoreContainer.setPlayerCount((int)e.playerCount);
                }
                else
                {
                    Debug.LogError("[DataCollector] no player count found in sessionEnd event");
                }
                scoreContainer.setLevelName(e.level);

                int s = (int)((wave - (int)wave) * 10000);
                Score += s;

                e.addScore(Score);

                if(GameManager.gameManagerInstance.CurrentGameMode == GameMode.YOLOMode)
                {
                    scoreContainer.setGameMode(GameMode.YOLOMode);
                    scoreContainer.setYoloTime(e.time);
                }
                else
                {
                    scoreContainer.setGameMode(GameMode.NormalMode);
                }

                if (DataCollector.instance.eventBuild)
                {
                    scoreContainer.setGameName(e.name);
                }
                

                Debug.Log(scoreContainer.ToString());
                /*
                Debug.Log("[DataCollector] wave " + wave);
                Debug.Log("[DataCollector] score " + Score);
                Debug.Log("[ScoreContainer] score " + scoreContainer.getScoreSum());
                */

                break;
        }
        //Debug.Log("[Score]" + (Score - scoreBefore));
        //intermediateScore += Score - scoreBefore;
    }
    
    public ScoreContainer getScoreContainer()
    {
        return scoreContainer;
    }

    // wicked workaround for 1 frame delayed score calc (problem with AccumulatedRessourceValue)
    public IEnumerator DisplayScoreCalcCoroutine(Event e)
    {
        yield return null;
        yield return null;
        calculateDisplayScore(e);
    }

    // check if server is reachable
    IEnumerator TestConnection()
    {
        float timeTaken = 0.0F;
        float maxTime = 2.0F;
        
        Ping testPing = new Ping(serverIP);

        timeTaken = 0.0F;

        while (!testPing.isDone)
        {

            timeTaken += Time.deltaTime;
                
            if (timeTaken > maxTime)
            {
                // if time has exceeded the max
                // time, break out and return false
                online = false;
                Debug.Log("[DataCollector] <color=red>OFFLINE</color>");
                break;
            }

            yield return null;
        }
        if (timeTaken <= maxTime) {
            online = true;
            Debug.Log("[DataCollector] <color=green>ONLINE</color>");
        }
        yield return null;
        
    }


    public void InvokeDisplayScoreCalcDelayed(Event e)
    {
        StartCoroutine(DisplayScoreCalcCoroutine(e));
    }

}

/*
  ////// EXAMPLE EVENT ////////
  new Event(Event.TYPE.ability).addPos(this.transform).addWave().addLevel().addCharacter(this.playerName).addPlayerCount().send();
*/
