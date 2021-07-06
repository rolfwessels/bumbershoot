using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bumbershoot.Utilities.Helpers
{
    public static class TaskHelper
    {
        public static void ContinueWithNoWait<TType>(this Task<TType> updateAllReferences,
            Action<Task<TType>> logUpdate)
        {
            updateAllReferences.ContinueWith(logUpdate);
        }

        public static void ContinueWithAndLogError(this Task sendAsync, Action<string, Exception> log = null)
        {
            sendAsync.ContinueWith(x =>
            {
                if (x.Exception != null) log?.Invoke($"Failed to run async method:{x.Exception.Message}", x.Exception);
            });
        }
    }
}