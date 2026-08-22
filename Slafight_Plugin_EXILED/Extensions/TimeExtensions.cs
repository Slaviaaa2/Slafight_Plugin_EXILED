using System;

namespace Slafight_Plugin_EXILED.Extensions;

public static class TimeExtensions
{
    /// <summary>
    /// 分を秒へ変換します。
    /// </summary>
    public static float MinutesToSeconds(this float minutes)
        => minutes * 60f;

    /// <summary>
    /// 秒を分へ変換します。
    /// </summary>
    public static float SecondsToMinutes(this float seconds)
        => seconds / 60f;

    /// <summary>
    /// 「分」から TimeSpan を生成します。
    /// 例: 11.75 → 00:11:45
    /// </summary>
    public static TimeSpan ToTimeSpanFromMinutes(this double minutes)
        => TimeSpan.FromMinutes(minutes);

    /// <summary>
    /// 「分」から TimeSpan を生成します。
    /// 例: 11.75 → 00:11:45
    /// </summary>
    public static TimeSpan ToTimeSpanFromMinutes(this float minutes)
        => TimeSpan.FromMinutes(minutes);

    /// <summary>
    /// 「分:秒」形式の文字列を秒へ変換します。
    /// 例: "11:45" → 705
    /// </summary>
    public static double ToSeconds(this string time)
    {
        if (!TimeSpan.TryParse($"00:{time}", out var result))
            throw new FormatException($"Invalid time format: {time}");

        return result.TotalSeconds;
    }

    /// <summary>
    /// 秒を「分:秒」形式へ変換します。
    /// 例: 705 → "11:45"
    /// </summary>
    public static string ToMinutesSeconds(this double seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);

        return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
    }

    /// <summary>
    /// 秒を「分:秒」形式へ変換します。
    /// 例: 705 → "11:45"
    /// </summary>
    public static string ToMinutesSeconds(this float seconds)
        => ((double)seconds).ToMinutesSeconds();
}