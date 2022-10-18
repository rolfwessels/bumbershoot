using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bumbershoot.Utilities.Helpers
{
    public static class TimerHelper
    {
        public static T WaitFor<T>(this T updateModels, Func<T, bool> o, int timeOut = 500)
        {
            var stopTime = DateTime.Now.AddMilliseconds(timeOut);
            bool result;
            do
            {
                result = o(updateModels);
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
            if (build.TotalDays >= 1) return $"{Math.Round(build.TotalDays, 1)}d";
            if (build.TotalHours >= 1) return $"{Math.Round(build.TotalHours, 1)}h";
            if (build.TotalMinutes >= 1) return $"{Math.Round(build.TotalMinutes, 1)}m";
            if (build.TotalMilliseconds >= 1000) return $"{Math.Round(build.TotalSeconds,1)}s";
            return $"{Math.Round(build.TotalMilliseconds)}ms";
        }

        public static void Retry<T>(this Action action, int count = 3, int retryDelay = 100, Action<T, int>? callBack = null) where T : Exception
        {
            Task Action()
            {
                return Task.Run(action);
            }
            RetryAsync(Action, count, retryDelay, callBack).Wait();
        }

        public static async Task RetryAsync<T>(this Func<Task> action, int count = 3, int retryDelay = 100 , Action<T,int>? callBack = null) where T : Exception
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
}