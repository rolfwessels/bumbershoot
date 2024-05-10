using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Helpers;

public static class TimerHelper
{
    public static async Task<T> WaitForAsync<T>(this T updateModels, Func<T, bool> o, int timeOut = 500)
    {
        var stopTime = DateTime.Now.AddMilliseconds(timeOut);
        var millisecondsTimeout = 1;
        bool result;
        do
        {
            result = o(updateModels);
            if (millisecondsTimeout == 0)
                millisecondsTimeout = 1;
            else await Task.Delay((millisecondsTimeout += millisecondsTimeout).Dump());
        } while (!result && DateTime.Now < stopTime);

        return updateModels;
    }

    public static T WaitFor<T>(this T updateModels, Func<T, bool> o, int timeOut = 500)
    {
        var stopTime = DateTime.Now.AddMilliseconds(timeOut);
        var millisecondsTimeout = 0;
        bool result;
        do
        {
            result = o(updateModels);
            if (millisecondsTimeout == 0)
                millisecondsTimeout = 1;
            else Thread.Sleep(millisecondsTimeout += millisecondsTimeout);
        } while (!result && DateTime.Now < stopTime);

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

    public static void Retry<T>(this Action action,
        int count = 3,
        int retryDelay = 100,
        Action<T, int>? callBack = null) where T : Exception
    {
        Task Action()
        {
            return Task.Run(action);
        }

        RetryAsync(Action, count, retryDelay, callBack).Wait();
    }

    public static async Task RetryAsync<T>(this Func<Task> action,
        int count = 3,
        int retryDelay = 100,
        Action<T, int>? callBack = null) where T : Exception
    {
        for (var tries = 0; tries < count; tries++)
            try
            {
                await action();
                return;
            }
            catch (T e)
            {
                if (tries + 1 == count) throw;
                callBack?.Invoke(e, retryDelay);
                await Task.Delay(retryDelay);
                retryDelay += retryDelay;
            }
    }
}