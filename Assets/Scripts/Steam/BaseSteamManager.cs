using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
// BaseSteamManager is the base class of SteamManager and SteamManagerDummy and regulates the initialization of those.
// The dummy class is initialized instead of the manager class when Unity is running in editor mode (and WIN Standalone atm for all following test builds).
// If testing the SteamManager, please enable your Steam Standalone and comment lines 20 and 22-24.
//
[DisallowMultipleComponent]
public class BaseSteamManager : MonoBehaviour {

    protected static BaseSteamManager instance;
    public static BaseSteamManager Instance
    {
        get
        {
            //TODO: COMMENT LINES 20 AND 22-24 IN BUILDS

            //#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
                return instance ?? new GameObject("SteamManager").AddComponent<SteamManager>();
            /*#else
                return instance ?? new GameObject("SteamManagerDummy").AddComponent<SteamManagerDummy>();
            #endif*/

            //ENDTODO
        }
    }

    protected static bool everInitialized;

    protected bool initialized;
    public static bool Initialized
    {
        get
        {
            return Instance.initialized;
        }
    }

    public delegate void LeaderboardAction(List<LeaderboardEntry> entries);

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (everInitialized)
        {
            throw new System.Exception("Tried to Initialize the SteamManager twice in one session!");
        }

        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        instance = null;

        if (!initialized)
        {
            return;
        }
    }

    protected virtual void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (!initialized)
        {
            return;
        }
    }

    public virtual void LogAchievementEvent (Event e) { }
	
	public virtual void LogAchievementData (AchievementID id) { }

    public virtual string GetSteamName(){ return ""; }

    public virtual string GetSteamID() { return ""; }

    public virtual int GetRank() { return 0; }

    public virtual void RequestLeaderboardEntries(string level, int playerCount, int from, int to, LeaderboardAction action) { }

    public virtual void ResetGame() { }

    //pause menu event
    public delegate void OnOverLayActivatedEvent();
    public virtual event OnOverLayActivatedEvent OnOverlayActivated;
}

//
// This is the SteamManagerDummy class, used in editor mode.
//
[DisallowMultipleComponent]
class SteamManagerDummy : BaseSteamManager
{
    protected override void Awake()
    {
        base.Awake();

        initialized = true;
        everInitialized = true;
    }
}
