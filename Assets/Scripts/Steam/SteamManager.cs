﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

//
// The SteamManager provides a base implementation of Steamworks.NET
// and handles the basics of starting up and shutting down the SteamAPI for use.
//
[DisallowMultipleComponent]
class SteamManager : MonoBehaviour
{
    private static SteamManager instance;
    private static SteamManager Instance
    {
        get
        {
            return instance ?? new GameObject("SteamManager").AddComponent<SteamManager>();
        }
    }

    private static bool everInitialized;

    private bool initialized;
    public static bool Initialized
    {
        get
        {
            return Instance.initialized;
        }
    }

    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
    private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    // game ID
    private CGameID gameID;

    // could the stats be retrieved from steam?
    private bool requestedStats;
    private bool statsValid;

    // should stats be stored this frame?
    private bool storeStats;

    // steam api callbacks
    protected Callback<GameOverlayActivated_t> GameOverlayActivated;
    protected Callback<UserStatsReceived_t> UserStatsReceived;
    protected Callback<UserStatsStored_t> UserStatsStored;
    protected Callback<UserAchievementStored_t> UserAchievementStored;
    private CallResult<LeaderboardFindResult_t> LeaderboardFindResult;
    private CallResult<LeaderboardScoreUploaded_t> LeaderboardScoreUploaded;

    private IDictionary<AchievementID, Achievement> achievements = new Dictionary<AchievementID, Achievement>();

    //current leaderboard handle
    private SteamLeaderboard_t currSteamLeaderboard;

    //persisted stats
    private int totalGamesPlayed;
    private int assesKilled;
    private int totalGameStarts;
    private int enemiesKilled;

    //current stats
    private float waveReached;
    private int playerCount;
    private bool smartEnough;
    private bool creditsViewed;
    private bool mangoPicked;
    private bool firstWaveDeath;
    private bool trapDeath;
    private bool mobileJoin;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (everInitialized)
        {
            // This is almost always an error.
            // The most common case where this happens is the SteamManager getting destroyed via Application.Quit() and having some code in some OnDestroy which gets called afterwards, creating a new SteamManager.
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }
        
        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the 
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/documentation/drm#FAQ
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        { // We catch this exception here, as it will be the first occurence of it.
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        // Initialize the SteamAPI, if Init() returns false this can happen for many reasons.
        // Some examples include:
        // Steam Client is not running.
        // Launching from outside of steam without a steam_appid.txt file in place.
        // Running under a different OS User or Access level (for example running "as administrator")
        // Valve's documentation for this is located here:
        // https://partner.steamgames.com/documentation/getting_started
        // https://partner.steamgames.com/documentation/example // Under: Common Build Problems
        // https://partner.steamgames.com/documentation/bootstrap_stats // At the very bottom

        // If you're running into Init issues try running DbgView prior to launching to get the internal output from Steam.
        // http://technet.microsoft.com/en-us/sysinternals/bb896647.aspx
        initialized = SteamAPI.Init();
        if (!initialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            return;
        }

        everInitialized = true;
    }
    
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (!initialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            // Set up our callback to recieve warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to recieve warnings.
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }

        // cache to compare in callbacks
        gameID = new CGameID(SteamUtils.GetAppID());

        // add arbitrary achievements
        achievements.Add(AchievementID.ACH_PLAY_21_GAMES, new Achievement("Half the truth", "Played 21 games."));
        achievements.Add(AchievementID.ACH_KILL_1000_ASSES, new Achievement("J'adore derrière", "Killed 1000 asses."));

        // register callbacks
        GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
        LeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
        LeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
        
        requestedStats = false;
        statsValid = false;

        Debug.Log("SteamStats enabled by " + SteamFriends.GetPersonaName() + " with AppID " + gameID);
    }

    /// <summary>
    /// Running SteamAPI callbacks and retrieving/storing user data.
    /// </summary>
    void Update()
    {
        if (!initialized)
            return;

        SteamAPI.RunCallbacks();

        if (!requestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done
            if (!initialized)
            {
                requestedStats = true;
                return;
            }

            // If yes, request our stats
            bool success = SteamUserStats.RequestCurrentStats();
            //statsValid = true;

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            requestedStats = success;
        }

        if (!statsValid)
            return;

        #region Unlock and store achievements
        
        //games played
        if (totalGamesPlayed == 21)
            UnlockAchievement(AchievementID.ACH_PLAY_21_GAMES);
        else if (totalGamesPlayed == 42)
            UnlockAchievement(AchievementID.ACH_PLAY_42_GAMES);

        //enemies killed
        if (assesKilled == 1000)
            UnlockAchievement(AchievementID.ACH_KILL_1000_ASSES);
        if (enemiesKilled == 2)
            UnlockAchievement(AchievementID.ACH_KILL_TWO_ENEMIES);

        //waves reached
        if(waveReached >= 10 && waveReached <= 19)
            UnlockAchievement(AchievementID.ACH_REACH_W10);
        else if (waveReached >= 20 && waveReached <= 29)
            UnlockAchievement(AchievementID.ACH_REACH_W20);
        else if (waveReached >= 30)
            UnlockAchievement(AchievementID.ACH_REACH_W30);

        //player count
        if(playerCount == 1)
            UnlockAchievement(AchievementID.ACH_PLAY_ALONE);
        else if (playerCount == 4)
            UnlockAchievement(AchievementID.ACH_PLAY_WITH_FOUR);

        //menu
        if (smartEnough)
            UnlockAchievement(AchievementID.ACH_SMART_ENOUGH_FOR_THE_MENU);
        if (creditsViewed)
            UnlockAchievement(AchievementID.ACH_CREDITS_VIEWED);
        if(totalGameStarts == 1)
            UnlockAchievement(AchievementID.ACH_STARTED_GAME_ONCE);
        else if (totalGameStarts == 3)
            UnlockAchievement(AchievementID.ACH_STARTED_GAME_THRICE);
        else if (totalGameStarts == 10)
            UnlockAchievement(AchievementID.ACH_STARTED_GAME_TEN_TIMES);

        //player death
        if (firstWaveDeath)
            UnlockAchievement(AchievementID.ACH_DIED_IN_W1);
        if (trapDeath)
            UnlockAchievement(AchievementID.ACH_DIED_IN_TRAP);

        //player join
        if (mobileJoin)
            UnlockAchievement(AchievementID.ACH_SMARTPHONE_JOIN);


        #endregion

        //Store stats in the Steam database if necessary
        if (storeStats)
        {
            SteamUserStats.SetStat("TotalGamesPlayed", totalGamesPlayed);
            SteamUserStats.SetStat("TotalGameStarts", totalGameStarts);
            SteamUserStats.SetStat("AssesKilled", assesKilled);
            SteamUserStats.SetStat("EnemiesKilled", enemiesKilled);

            bool success = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try again later.
            storeStats = !success;
        }

    }

    /// <summary>
    /// This method unlocks achievements, if found - and not already unlocked.
    /// </summary>
    /// <param name="id">The achievement ID of the achievement to be unlocked.</param>
    private void UnlockAchievement(AchievementID id)
    {
        Achievement ach;
        bool found = achievements.TryGetValue(id, out ach);

        if (found && !ach.achieved)
        {
            ach.achieved = true;

            // the icon may change once it's unlocked
            //achievement.iconImage = 0;

            // mark it down
            SteamUserStats.SetAchievement(id.ToString());

            // store stats
            storeStats = true;
        }
        else if (!found)
        {
            Debug.Log("AchievementID not found in Dictionary");
        }
    }

    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        if (!((ulong)gameID == pCallback.m_nGameID))
            return;

        if (0 == pCallback.m_nMaxProgress)
            Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
        else
            Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
    }
    
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        if (!((ulong)gameID == pCallback.m_nGameID))
            return;

        if (EResult.k_EResultOK == pCallback.m_eResult)
        {
            Debug.Log("StoreStats - success");
        }
        else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
        {
            // One or more stats we set broke a constraint. They've been reverted,
            // and we should re-iterate the values now to keep in sync.
            Debug.Log("StoreStats - some failed to validate");
            // fake up a callback so that we re-load the values
            UserStatsReceived_t callback = new UserStatsReceived_t();
            callback.m_eResult = EResult.k_EResultOK;
            callback.m_nGameID = (ulong)gameID;
            OnUserStatsReceived(callback);
        }
        else
        {
            Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
        }
    }
    
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!((ulong)gameID == pCallback.m_nGameID))
            return;

        if (EResult.k_EResultOK == pCallback.m_eResult)
        {
            Debug.Log("Received stats and achievements from Steam");

            statsValid = true;

            // load achievements
            foreach (KeyValuePair<AchievementID, Achievement> entry in achievements)
            {
                bool ret = SteamUserStats.GetAchievement(entry.Key.ToString(), out entry.Value.achieved);
                if (ret)
                {
                    entry.Value.name = SteamUserStats.GetAchievementDisplayAttribute(entry.Key.ToString(), "name");
                    entry.Value.desc = SteamUserStats.GetAchievementDisplayAttribute(entry.Key.ToString(), "desc");
                }
                else
                {
                    Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + entry.Key + "\nIs it registered in the Steam Partner site?");
                }
            }

            // load stats
            SteamUserStats.GetStat("TotalGamesPlayed", out totalGamesPlayed);
            SteamUserStats.GetStat("TotalGameStarts", out totalGameStarts);
            SteamUserStats.GetStat("AssesKilled", out assesKilled);
            SteamUserStats.GetStat("EnemiesKilled", out enemiesKilled);
        }
        else
        {
            Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
        }
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        if (pCallback.m_bActive != 0)
        {
            Debug.Log("Steam Overlay has been activated");
        }
        else
        {
            Debug.Log("Steam Overlay has been closed");
        }
    }

    private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bLeaderboardFound != 0)
        {
            currSteamLeaderboard = pCallback.m_hSteamLeaderboard;
        }
    }
    
    private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + LeaderboardScoreUploaded_t.k_iCallback + " - LeaderboardScoreUploaded] - " + pCallback.m_bSuccess + " -- " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_nScore + " -- " + pCallback.m_bScoreChanged + " -- " + pCallback.m_nGlobalRankNew + " -- " + pCallback.m_nGlobalRankPrevious);
    }

    /// <summary>
    /// This method can be used by the DataCollector to send Data to the SteamManager.
    /// </summary>
    /// <param name="e">The Event that is sent by the DataCollector.</param>
    public void logAchievementEvent(Event e)
    {
        switch (e.type)
        {
            case Event.TYPE.gameStart:
                playerCount = (int)e.playerCount;
                smartEnough = true;
                break;
            case Event.TYPE.ability:
                break;
            case Event.TYPE.death:
                if (e.wave >= 1 && e.wave < 2)
                    firstWaveDeath = true;
                if (e.enemy.Equals("laser_trap"))
                    trapDeath = true;
                break;
            case Event.TYPE.join:
                if (e.cause.Equals("smartphone"))
                    mobileJoin = true;
                break;
            case Event.TYPE.kill:
                enemiesKilled++;
                if (e.enemy.Equals("MeleeVeryWeak"))
                    assesKilled++;
                break;
            case Event.TYPE.powerup:
                break;
            case Event.TYPE.superAbility:
                break;
            case Event.TYPE.sessionEnd:
                totalGamesPlayed++;
                waveReached = (float)e.wave;
                //save leaderboard entry
                SteamAPICall_t handle = SteamUserStats.FindLeaderboard(e.level + " - " + e.mode + " - " + e.playerCount + " players");
                LeaderboardFindResult.Set(handle);
                if(e.mode.Equals("normal"))
                    SteamUserStats.UploadLeaderboardScore(currSteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (int)waveReached, null, 0);
                if (e.mode.Equals("yolo"))
                    SteamUserStats.UploadLeaderboardScore(currSteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, e.time, null, 0);
                //store new persisted stats next frame
                storeStats = true;
                break;
        }
    }

    public void logAchievementData(AchievementID id)
    {
        switch (id)
        {
            case AchievementID.ACH_STARTED_GAME_ONCE:
                totalGameStarts++;
                break;
            case AchievementID.ACH_CREDITS_VIEWED:
                creditsViewed = true;
                break;
            case AchievementID.ACH_PICK_SPACETIME_MANGO:
                mangoPicked = true;
                break;
        }
    }

    /// <summary>
    // OnApplicationQuit gets called too early to shutdown the SteamAPI. Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be guarenteed upon Shutdown. Prefer OnDisable().
    /// </summary>
    private void OnDestroy()
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

        SteamAPI.Shutdown();
    }
}
