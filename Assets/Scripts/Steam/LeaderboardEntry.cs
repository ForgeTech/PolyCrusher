using UnityEngine;
using System.Collections;
using System;
using Steamworks;

public class LeaderboardEntry
{
    public string steamName;
    public CSteamID steamID;
    public int rank;
    public int score;
    public int wave;
    public int year;
    public int month;
    public int day;

    /// <param name="steamName">The name of the Steam User as displayed in the community.</param>
    /// <param name="steamName">The name of the Steam ID of this Steam User.</param>
    /// <param name="rank">The online rank in this Leaderboard.</param>
    /// <param name="score">The score in this Leaderboard.</param>
    /// <param name="wave">The wave in this Leaderboard.</param>
    /// <param name="year">The year of this Leaderboardentry.</param>
    /// <param name="month">The month of this Leaderboardentry.</param>
    /// <param name="day">The day of this Leaderboardentry.</param>
    public LeaderboardEntry(string steamName, CSteamID steamID, int rank, int score, int wave, int year, int month, int day)
    {
        this.steamName = steamName;
        this.steamID = steamID;
        this.rank = rank;
        this.score = score;
        this.wave = wave;
        this.year = year;
        this.month = month;
        this.day = day;
    }
}
