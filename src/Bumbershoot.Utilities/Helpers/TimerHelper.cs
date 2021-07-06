using System;

namespace Bumbershoot.Utilities.Helpers
{
    public static class TimerHelper
    {
        public static T WaitFor<T>(this T updateModels, Func<T, bool> o, int timeOut = 500)
        {
            var stopTime = DateTime.Now.AddMilliseconds(timeOut);
            var result = false;

            do
            {
                result = o(updateModels);
            } while (!result && DateTime.Now < stopTime);

            return updateModels;
        }
    }
}