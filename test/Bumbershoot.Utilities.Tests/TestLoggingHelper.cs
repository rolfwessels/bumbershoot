using System;
using System.Reflection;
using Serilog;
using NUnit.Framework;
using Serilog.Events;

namespace Bumbershoot.Utilities.Tests
{
    [SetUpFixture]
    public class TestLoggingHelper
    {
        private static Lazy<ILogger> _logger;

        static TestLoggingHelper()
        {
            _logger = new Lazy<ILogger>(SetupOnce);
        }

        public static void EnsureExists()
        {
            Log.Logger = _logger.Value;
        }

        private static ILogger SetupOnce()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}