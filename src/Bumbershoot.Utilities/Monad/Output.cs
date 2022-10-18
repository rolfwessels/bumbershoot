using System;

namespace Bumbershoot.Utilities.Monad;

public class Output<T>
{
    public class Success : Output<T>
    {
        public T Value { get; }

        public Success(T value)
        {
            Value = value;
        }
    }

    public class Failed : Output<T> 
    {
        public Exception Exception { get; }

        public Failed(Exception exception)
        {
            Exception = exception;
        }
    }
    public bool IsSuccess => this is Success;
    public bool IsFailed => this is Failed;
    public Success? AsSuccess => this as Success;
    public Exception? FailedException => (this as Failed)?.Exception;


    public Output<T> Then(Action<T> action)
    {
        if (this is Success success)
        {
            action(success.Value);
        }
        return this;
    }

    public Output<T2> ThenMap<T2>(Func<T,T2> action)
    {
        try
        {
            if (this is Success success)
            {
                var action1 = action(success.Value);
                return new Output<T2>.Success(action1);
            }
        }
        catch (Exception e)
        {
            return new Output<T2>.Failed(e);
        }
        return new Output<T2>.Failed(FailedException!);
    }

    public Output<T> Else(Action<Exception> action)
    {
        if (this is Failed failed)
        {
            action(failed.Exception);
        }

        return this;
    }
}

public class Output : Output<bool>
{
    public static Output<bool> Ok()
    {
        return new Success(true);
    }

    public static Output<T> Ok<T>(T value)
    {
        return new Output<T>.Success(value);
    }

    public static Output<T> Failure<T>(string message)
    {
        var exception = new Exception(message);
        return Failure<T>(exception);
    }

    private static Output<T> Failure<T>(Exception exception)
    {
        return new Output<T>.Failed(exception);
    }

}