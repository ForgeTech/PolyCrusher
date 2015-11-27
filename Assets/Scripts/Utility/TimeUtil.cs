using UnityEngine;
using System.Collections;

public class TimeUtil
{
    protected int hour;

    protected int minute;

    protected int second;

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


    public TimeUtil(int hour, int minute, int second)
    {
        this.hour = hour;
        this.minute = minute;
        this.second = second;
    }

    public TimeUtil() : this(0, 0, 0)
    {
    }

    /// <summary>
    /// Converts seconds to a time format.
    /// </summary>
    /// <param name="seconds">Seconds</param>
    /// <returns>Time format.</returns>
    public static TimeUtil SecondsToTime(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds / 60) % 60;
        int s = seconds % 60;

        return new TimeUtil(h, m, s);
    }
}