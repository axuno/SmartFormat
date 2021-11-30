using System;
using System.IO;
using NUnit.Framework;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // Disable console output from test methods
            Console.SetOut(TextWriter.Null);

            SmartSettings.IsThreadSafeMode = false;
            PoolSettings.IsPoolingEnabled = true;
            PoolSettings.CheckReturnedObjectsExistInPool = true;

            // After the global settings, we can change settings for individual pools.
            // E.g.: FormatDetailsPool.Instance.Pool.IsPoolingEnabled = false;
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            // Nothing defined here
        }
    }
}