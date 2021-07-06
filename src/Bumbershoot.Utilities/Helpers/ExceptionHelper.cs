using System;
using System.Linq;

namespace Bumbershoot.Utilities.Helpers
{
    public static class ExceptionHelper
    {
        public static Exception ToSimpleException(this AggregateException exception)
        {
            if (exception.InnerExceptions.Count == 1) return exception.InnerExceptions.First();
            return new Exception(exception.InnerExceptions.Select(x => x.Message).StringJoin(), exception);
        }

        public static string ToSingleExceptionString(this Exception exception)
        {
            var simpleException = exception is AggregateException aggregateException
                ? aggregateException.ToSimpleException()
                : exception;
            return simpleException.Message + Environment.NewLine + simpleException.StackTrace;
        }

        public static Exception ToFirstExceptionOfException(this Exception exception)
        {
            if (exception is AggregateException aggregateException) return aggregateException.ToSimpleException();
            return exception;
        }
    }
}