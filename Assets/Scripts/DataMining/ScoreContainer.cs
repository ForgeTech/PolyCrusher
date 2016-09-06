using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ScoreContainer
{
    private int bossKills = 0;
    private int polysTriggered = 0;
    private int polyKills = 0;
    private int cutKills = 0;
    private int powerupsCollected = 0;
    private int playerRevivals = 0;
    private GameMode gameMode = GameMode.NormalMode;
    private int playerCount = 0;
    private string levelname = "";
    private float wave = 0;
    private int yoloTime = 0;
    private string gameName = "";

    public void addBossKills(int x)
    {
        bossKills += x;
    }

    public void addPolysTriggered(int x)
    {
        polysTriggered += x;
    }

    public void addPolyKills(int x)
    {
        polyKills += x;
    }

    public void addCutKills(int x)
    {
        cutKills += x;
    }

    public void addPowerupsCollected(int x)
    {
        powerupsCollected += x;
    }

    public void addPlayerRevials(int x)
    {
        playerRevivals += x;
    }

    public void setYoloTime(int x)
    {
        yoloTime = x;
    }

    public void setWave(float x)
    {
        wave = x;
    }

    public void setPlayerCount(int x)
    {
        playerCount = x;
    }

    public void setGameMode(GameMode mode)
    {
        gameMode = mode;
    }

    public void setLevelName(string name)
    {
        levelname = name;
    }

    public void setGameName(string name)
    {
        gameName = name;
    }


    //--------------------------


    public int getBossKills()
    {
        return bossKills;
    }

    public int getPolysTriggered()
    {
        return polysTriggered;
    }

    public int getPolyKills() {
        return polyKills;
    }

    public int getCutKills()
    {
        return cutKills;
    }

    public int getPowerupsCollected()
    {
        return powerupsCollected;
    }

    public int getPlayerRevivals()
    {
        return playerRevivals;
    }

    public float getWave()
    {
        return wave;
    }

    public float getWaveMultiplier()
    {
        return wave - 1;
    }

    public int getPlayerCount()
    {
        return playerCount;
    }

    public GameMode getGameMode()
    {
        return gameMode;
    }

    public string getLevelName()
    {
        return levelname;
    }

    public string getGameName()
    {
        return gameName;
    }


    //------score getter------

    public int getBossKillsScore()
    {
        return bossKills * 2500;
    }

    public int getPolysTriggeredScore()
    {
        return polysTriggered * 1000;
    }

    public int getPolyKillsScore()
    {
        return polyKills * 100;
    }

    public int getCutKillsScore()
    {
        return cutKills * 100;
    }

    public int getPowerupsCollectedScore()
    {
        return powerupsCollected * 100;
    }

    public int getPlayerRevivalsScore()
    {
        if (getGameMode() == GameMode.NormalMode)
            return playerRevivals * -1000;
        else
            return 0;
    }

    /// <summary>
    /// (float Wave - 1) * 10000
    /// </summary>
    public int getWaveScore()
    {
        if (wave == 0)
            return 0;
        else
            return (int)((wave - 1) * 10000);
    }

    public int getScoreSum()
    {   if (getGameMode() == GameMode.NormalMode)
            return getBossKillsScore() + getPolysTriggeredScore() + getPolyKillsScore() + getCutKillsScore() + getPowerupsCollectedScore() + getPlayerRevivalsScore() + getWaveScore();
        else
            return getBossKillsScore() + getPolysTriggeredScore() + getPolyKillsScore() + getCutKillsScore() + getPowerupsCollectedScore() + getPlayerRevivalsScore() + getYoloScore();
    }

    public int getYoloScore()
    {
        return yoloTime;
    }

    public override string ToString()
    {
        string str = "[ScoreContainer]";
        str += " playercount: " + getPlayerCount();
        str += " level: " + getLevelName();
        str += " yoloscore: " + getYoloScore();
        str += " score: " + getScoreSum();
        str += " mode: " + getGameMode().ToString();
        return str;
    }
}

