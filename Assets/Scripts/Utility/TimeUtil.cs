using UnityEngine;
using System.Collections;

public class TimeUtil
{
    protected int hour;

    protected int minute;

    protected int second;

    protected int milliseconds;

    /// <summary>
    ///  Gets the hour.
    /// </summary>
    public int Hour
    {
        get { return this.hour; }
    }

    /// <summary>
    /// Gets the minute.
    /// </summary>
    public int Minute
    {
        get { return this.minute; }
    }

    /// <summary>
    /// Gets the second.
    /// </summary>
    public int Second
    {
        get { return this.second; }
    }

    /// <summary>
    /// Gets the milliseconds
    /// </summary>
    public int Milliseconds
    {
        get { return this.milliseconds; }
    }

    /// <summary>
    /// Gets the total time in milliseconds.
    /// </summary>
    public int TotalTime
    {
        get
        {
            int sumTime = 0;

            sumTime += this.Milliseconds;
            sumTime += this.Second * 1000;
            sumTime += this.Minute * 60 * 1000;
            sumTime += this.Hour * 60 * 60 * 1000;

            return sumTime;
        }
    }

    public TimeUtil(int hour, int minute, int second, int milliseconds)
    {
        this.hour = hour;
        this.minute = minute;
        this.second = second;
        this.milliseconds = milliseconds;
    }

    public TimeUtil() : this(0, 0, 0, 0)
    {
    }

    /// <summary>
    /// Converts seconds to a time format.
    /// </summary>
    /// <param name="seconds">Seconds</param>
    /// <returns>Time format.</returns>
    public static TimeUtil SecondsToTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds / 60) % 60);
        int s = (int)(seconds % 60);
        int ms = (int)((seconds - Mathf.Floor(seconds)) * 1000);

        return new TimeUtil(h, m, s, ms);
    }
}