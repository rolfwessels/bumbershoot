using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Helpers;

public static class TimerHelper
{
    private const double DelayMultiplier = 1.5;

    public static async Task<T> WaitForAsync<T>(this T updateModels,
        Func<T, bool> o,
        int timeoutMilliseconds = 500,
        int initialDelayMilliseconds = 10)
    {
        var stopTime = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        double currentDelay = initialDelayMilliseconds;
        bool result;
        do
        {
            result = o(updateModels);
            if (result) continue;
            await Task.Delay((int)currentDelay);
            currentDelay = Math.Min(currentDelay * DelayMultiplier, timeoutMilliseconds);
        } while (!result && DateTime.UtcNow < stopTime);

        return updateModels;
    }

    public static async Task<TResult> WaitForAsync<T, TResult>(
        this T source,
        Func<T, Task<TResult>> getResultAsync,
        Func<TResult, bool> isValid,
        int timeoutMilliseconds = 3000,
        int initialDelayMilliseconds = 10)
    {
        var stopTime = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        double currentDelay = initialDelayMilliseconds;
        TResult? result = default;
        while (DateTime.UtcNow < stopTime)
        {
            result = await getResultAsync(source);
            if (isValid(result))
                break;
            await Task.Delay((int)currentDelay);
            currentDelay = Math.Min(currentDelay * DelayMultiplier, timeoutMilliseconds);
        }

        return result!;
    }

    public static T WaitFor<T>(this T updateModels,
        Func<T, bool> o,
        int timeoutMilliseconds = 500,
        int initialDelayMilliseconds = 10)
    {
        var stopTime = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        double currentDelay = initialDelayMilliseconds;
        bool result;
        do
        {
            result = o(updateModels);
            if (result) continue;
            Thread.Sleep((int)currentDelay);
            currentDelay = Math.Min(currentDelay * DelayMultiplier, timeoutMilliseconds);
        } while (!result && DateTime.UtcNow < stopTime);

        return updateModels;
    }

    public static T With<T>(this T build, Action<T> func)
    {
        func(build);
        return build;
    }

    public static string ShortTime(this TimeSpan build)
    {
        if (build.TotalDays >= 1)
            return $"{Math.Round(build.TotalDays, digits: 1).ToString(CultureInfo.InvariantCulture)}d";
        if (build.TotalHours >= 1)
            return $"{Math.Round(build.TotalHours, digits: 1).ToString(CultureInfo.InvariantCulture)}h";
        if (build.TotalMinutes >= 1)
            return $"{Math.Round(build.TotalMinutes, digits: 1).ToString(CultureInfo.InvariantCulture)}m";
        if (build.TotalMilliseconds >= 1000)
            return $"{Math.Round(build.TotalSeconds, digits: 1).ToString(CultureInfo.InvariantCulture)}s";
        return $"{Math.Round(build.TotalMilliseconds).ToString(CultureInfo.InvariantCulture)}ms";
    }
}