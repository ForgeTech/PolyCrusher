using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

//
// The SteamManager provides a base implementation of Steamworks.NET
// and handles the basics of starting up and shutting down the SteamAPI for use.
//
[DisallowMultipleComponent]
class SteamManager : ISteamManager
{
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

    //achievements
    private IDictionary<AchievementID, Achievement> achievements = new Dictionary<AchievementID, Achievement>();

    //current leaderboard handle
    private SteamLeaderboard_t currSteamLeaderboard;

    //persisted stats
    private int totalGamesPlayed;
    private int totalGameStarts;

    private int charactersPlayed;
    private bool charactersPlayedAchieved = false;

    private int assesKilled;
    private bool assesKilledAchieved = false;

    private int enemiesKilled;
    private bool enemiesKilledAchieved = false;

    private int enemiesCut;
    private bool enemiesCutAchieved = false;

    private int bulletsShot;
    private bool bulletsShotAchieved = false;

    //current stats
    private IDictionary<string, int> characterPowerups = new Dictionary<string, int>();
    private int totalPowerups;

    protected override void Awake()
    {
        base.Awake();

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

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_SteamAPIWarningMessageHook == null)
        {
            // Set up our callback to recieve warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to recieve warnings.
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }

        // cache to compare in callbacks
        gameID = new CGameID(SteamUtils.GetAppID());

        // add achievements
        achievements.Add(AchievementID.ACH_PLAY_21_GAMES, new Achievement("Half the truth", "Play 21 games."));
        achievements.Add(AchievementID.ACH_PLAY_42_GAMES, new Achievement("The truth - but what was the question?", "Play 42 games."));
        achievements.Add(AchievementID.ACH_KILL_1000_ASSES, new Achievement("J'adore derrière", "Kill 1000 asses."));
        achievements.Add(AchievementID.ACH_CURRENT_HIGHSCORE, new Achievement("15 minutes of fame", "Rank one on any leaderboard!"));
        achievements.Add(AchievementID.ACH_PLAY_ALL_CHARACTERS, new Achievement("Schizophrenia", "Play with all characters."));
        achievements.Add(AchievementID.ACH_PLAY_WITH_FOUR, new Achievement("Polyparty", "Play a game with three friends."));
        achievements.Add(AchievementID.ACH_PLAY_ALONE, new Achievement("Lone Wolf", "Play a game alone."));
        achievements.Add(AchievementID.ACH_GET_ALL_POWERUPS, new Achievement("I drink your milkshake", "Pick up all powerups in a coop game."));
        achievements.Add(AchievementID.ACH_CUT_100_ENEMIES, new Achievement("Cutting Edge", "Cut 100 enemies with the cutting powerup."));
        achievements.Add(AchievementID.ACH_CREDITS_VIEWED, new Achievement("Ultimate curiosity", "View the credits."));
        achievements.Add(AchievementID.ACH_PICK_SPACETIME_MANGO, new Achievement("The great flush", "Pick the arcane Space-Time-Mango in tutorial."));
        achievements.Add(AchievementID.ACH_A_MILLION_SHOTS, new Achievement("A million shots", "Fire a million bullets."));
        achievements.Add(AchievementID.ACH_KILL_TWO_ENEMIES, new Achievement("Second blood", "Kill two enemies."));
        achievements.Add(AchievementID.ACH_DIED_IN_W1, new Achievement("Sean Beaned", "Die in the very first wave."));
        achievements.Add(AchievementID.ACH_STARTED_GAME_ONCE, new Achievement("Bronze Medal", "Start game once."));
        achievements.Add(AchievementID.ACH_STARTED_GAME_THRICE, new Achievement("Silver Medal", "Start game thrice."));
        achievements.Add(AchievementID.ACH_STARTED_GAME_TEN_TIMES, new Achievement("Gold Medal", "Start game ten times."));
        achievements.Add(AchievementID.ACH_SMARTPHONE_JOIN, new Achievement("Wireless Killer", "Play the game with your smartphone."));
        achievements.Add(AchievementID.ACH_REACH_W10, new Achievement("Newbie", "Reach wave 10."));
        achievements.Add(AchievementID.ACH_REACH_W20, new Achievement("Professional Polycrusher", "Reach wave 20."));
        achievements.Add(AchievementID.ACH_REACH_W30, new Achievement("Ultimate Game Master", "Reach wave 30."));
        achievements.Add(AchievementID.ACH_KILL_B055_WITH_CHICKEN, new Achievement("Parricide", "Kill B055 with a chicken."));
        achievements.Add(AchievementID.ACH_KILL_B055_WITH_CUTTING, new Achievement("Chicken Chop Suey", "Kill B055 with the cutting powerup."));
        achievements.Add(AchievementID.ACH_KILL_40_ENEMIES_WITH_POLY, new Achievement("Sentenced to death", "Kill 40 enemies with one poly."));
        achievements.Add(AchievementID.ACH_SMART_ENOUGH_FOR_THE_MENU, new Achievement("Menu Whizz Kid", "Smart enough for the menu!"));
        achievements.Add(AchievementID.ACH_SURVIVE_YOLO_5_MINUTES, new Achievement("Survival Camp", "Survive yolo-mode for longer than 5 minutes."));
        achievements.Add(AchievementID.ACH_LAST_MAN_STANDING, new Achievement("Last Man Standing", "In a 4 player game just one survives the wave with 10% health."));
        achievements.Add(AchievementID.ACH_DIED_IN_TRAP, new Achievement("Captain Obvious", "Die in a trap."));
        achievements.Add(AchievementID.ACH_NO_DAMAGE_UNTIL_W10, new Achievement("Halfgodlike", "Don't take any damage until wave 10."));

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

#region unlock persisted achievements

        //games played
        if (totalGamesPlayed == 21)
            UnlockAchievement(AchievementID.ACH_PLAY_21_GAMES);
        else if (totalGamesPlayed == 42)
            UnlockAchievement(AchievementID.ACH_PLAY_42_GAMES);

        //enemies killed
        if (assesKilled >= 1000 && !assesKilledAchieved)
        {
            UnlockAchievement(AchievementID.ACH_KILL_1000_ASSES);
            assesKilledAchieved = true;
        }
        if (enemiesKilled >= 2 && !enemiesKilledAchieved)
        {
            UnlockAchievement(AchievementID.ACH_KILL_TWO_ENEMIES);
            enemiesKilledAchieved = true;
        }
        if (enemiesCut >= 100 && !enemiesCutAchieved)
        {
            UnlockAchievement(AchievementID.ACH_CUT_100_ENEMIES);
            enemiesCutAchieved = true;
        }

        //players
        if (charactersPlayed == 63 && !charactersPlayedAchieved) //bitwise filled int -> 111111
        {
            UnlockAchievement(AchievementID.ACH_PLAY_ALL_CHARACTERS);
            charactersPlayedAchieved = true;
        }

        //menu
        if (totalGameStarts == 1)
            UnlockAchievement(AchievementID.ACH_STARTED_GAME_ONCE);
        else if (totalGameStarts == 3)
            UnlockAchievement(AchievementID.ACH_STARTED_GAME_THRICE);
        else if (totalGameStarts == 10)
            UnlockAchievement(AchievementID.ACH_STARTED_GAME_TEN_TIMES);

        //bullets shot
        if (bulletsShot >= 1000000 && !bulletsShotAchieved)
        {
            UnlockAchievement(AchievementID.ACH_A_MILLION_SHOTS);
            bulletsShotAchieved = true;
        }

#endregion

        //Store stats in the Steam database if necessary
        if (storeStats)
        {
            SteamUserStats.SetStat("TotalGamesPlayed", totalGamesPlayed);
            SteamUserStats.SetStat("TotalGameStarts", totalGameStarts);
            SteamUserStats.SetStat("AssesKilled", assesKilled);
            SteamUserStats.SetStat("EnemiesKilled", enemiesKilled);
            SteamUserStats.SetStat("EnemiesCut", enemiesCut);
            SteamUserStats.SetStat("BulletsShot", bulletsShot);

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
            Debug.Log("Achievement with ID " + id.ToString() + " unlocked");

            // store stats
            storeStats = true;
        }
        else if (!found)
        {
            Debug.Log("AchievementID not found in Dictionary");
        }
    }

#region SteamAPI callbacks

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
            SteamUserStats.GetStat("EnemiesCut", out enemiesCut);
            SteamUserStats.GetStat("BulletsShot", out bulletsShot);
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

        if (pCallback.m_nGlobalRankNew == 1) //still to test - is 1 the top rank?
            UnlockAchievement(AchievementID.ACH_CURRENT_HIGHSCORE);
    }

#endregion

#region log and unlock current achievements

    /// <summary>
    /// This method can be used by the DataCollector to send Data to the SteamManager.
    /// </summary>
    /// <param name="e">The Event that is sent by the DataCollector.</param>
    public override void LogAchievementEvent(Event e)
    {
        switch (e.type)
        {
            case Event.TYPE.gameStart:
                PerformGameStartActions(e);
                break;
            case Event.TYPE.death:
                if (e.wave >= 1 && e.wave < 2)
                    UnlockAchievement(AchievementID.ACH_DIED_IN_W1);
                if (e.enemy.Equals("laser_trap"))
                    UnlockAchievement(AchievementID.ACH_DIED_IN_TRAP);
                break;
            case Event.TYPE.join:
                if (e.cause.Equals("smartphone"))
                    UnlockAchievement(AchievementID.ACH_SMARTPHONE_JOIN);
                break;
            case Event.TYPE.kill:
                enemiesKilled++;
                if (e.enemy.Equals("MeleeVeryWeak"))
                    assesKilled++;
                if (e.enemy.Equals("B055"))
                {
                    if (e.cause.Equals("ChickenAbility"))
                        UnlockAchievement(AchievementID.ACH_KILL_B055_WITH_CHICKEN);
                    else if (e.character.Equals("_LineManager"))
                        UnlockAchievement(AchievementID.ACH_KILL_B055_WITH_CUTTING);
                }
                if (e.character.Equals("_LineManager"))
                    enemiesCut++;
                break;
            case Event.TYPE.powerup:
                foreach (KeyValuePair<string, int> entry in characterPowerups)
                {
                    if (e.character.Equals(entry.Key))
                        characterPowerups[entry.Key]++;
                }
                totalPowerups++;
                break;
            case Event.TYPE.superAbility:
                if (e.kills >= 40)
                    UnlockAchievement(AchievementID.ACH_KILL_40_ENEMIES_WITH_POLY);
                break;
            case Event.TYPE.sessionEnd:
                PerformGameEndActions(e);
                break;
        }
    }

    public override void LogAchievementData(AchievementID id)
    {
        switch (id)
        {
            case AchievementID.ACH_STARTED_GAME_ONCE:
                totalGameStarts++;
                break;
            case AchievementID.ACH_CREDITS_VIEWED:
                UnlockAchievement(AchievementID.ACH_CREDITS_VIEWED);
                break;
            case AchievementID.ACH_PICK_SPACETIME_MANGO:
                UnlockAchievement(AchievementID.ACH_PICK_SPACETIME_MANGO);
                break;
            case AchievementID.ACH_A_MILLION_SHOTS:
                bulletsShot++;
                break;
            case AchievementID.ACH_LAST_MAN_STANDING:
                UnlockAchievement(AchievementID.ACH_LAST_MAN_STANDING);
                break;
            case AchievementID.ACH_NO_DAMAGE_UNTIL_W10:
                UnlockAchievement(AchievementID.ACH_NO_DAMAGE_UNTIL_W10);
                break;
        }
    }

    /// <summary>
    /// Performs all actions related to the game start event.
    /// </summary>
    /// <param name="e">The game start event.</param>
    private void PerformGameStartActions(Event e)
    {
        //playercount achievements
        if (e.playerCount == 1)
            UnlockAchievement(AchievementID.ACH_PLAY_ALONE);
        else if (e.playerCount == 4)
            UnlockAchievement(AchievementID.ACH_PLAY_WITH_FOUR);

        //smart enough = game started
        UnlockAchievement(AchievementID.ACH_SMART_ENOUGH_FOR_THE_MENU);

        foreach (string character in e.characters)
        {
            switch (character) //fill bitwise to save info in just one int
            {
                case "Timeshifter":
                    if ((charactersPlayed & 1) != 1)
                        charactersPlayed += 1;
                    break;
                case "Birdman":
                    if ((charactersPlayed & 2) != 2)
                        charactersPlayed += 2;
                    break;
                case "Charger":
                    if ((charactersPlayed & 4) != 4)
                        charactersPlayed += 4;
                    break;
                case "Fatman":
                    if ((charactersPlayed & 8) != 8)
                        charactersPlayed += 8;
                    break;
                case "Babuschka":
                    if ((charactersPlayed & 16) != 16)
                        charactersPlayed += 16;
                    break;
                case "Pantomime":
                    if ((charactersPlayed & 32) != 32)
                        charactersPlayed += 32;
                    break;
            }
            characterPowerups.Add(character, 0);
        }
    }

    /// <summary>
    /// Performs all actions related to the game end event.
    /// </summary>
    /// <param name="e">The game end event.</param>
    private void PerformGameEndActions(Event e)
    {
        //update played games
        totalGamesPlayed++;
        
        //waves reached achievement
        if (e.wave >= 10)
            UnlockAchievement(AchievementID.ACH_REACH_W10);
        if (e.wave >= 20)
            UnlockAchievement(AchievementID.ACH_REACH_W20);
        if (e.wave >= 30)
            UnlockAchievement(AchievementID.ACH_REACH_W30);

        //save leaderboard entry
        SteamAPICall_t handle = SteamUserStats.FindLeaderboard(e.level + " - " + e.mode + " - " + e.playerCount + " players");
        LeaderboardFindResult.Set(handle);
        int[] additionalInfo = new int[4] { (int)e.wave, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day };
        if (e.mode.Equals("normal"))
        {
            SteamUserStats.UploadLeaderboardScore(currSteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (int)e.wave, additionalInfo, additionalInfo.Length);
        }
        if (e.mode.Equals("yolo"))
        {
            SteamUserStats.UploadLeaderboardScore(currSteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, e.time, additionalInfo, additionalInfo.Length);
            if (e.time / 60000f >= 5f)
                UnlockAchievement(AchievementID.ACH_SURVIVE_YOLO_5_MINUTES);
        }

        //powerup achievement
        foreach (KeyValuePair<string, int> entry in characterPowerups)
        {
            if (entry.Value == totalPowerups && characterPowerups.Count > 1)
                UnlockAchievement(AchievementID.ACH_GET_ALL_POWERUPS);
        }
        characterPowerups.Clear();
        totalPowerups = 0;

        //store new persisted stats next frame
        storeStats = true;
    }
    
#endregion

    /// <summary>
    // OnApplicationQuit gets called too early to shutdown the SteamAPI. Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be guarenteed upon Shutdown. Prefer OnDisable().
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        SteamAPI.Shutdown();
    }
}
