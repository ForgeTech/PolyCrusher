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
    private float wave = 0;

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

    public float getWave()
    {
        return wave;
    }

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

    public float getWaveScore()
    {
        return wave * 10000;
    }
}

