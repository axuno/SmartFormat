using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class TimeFormatterTests
    {
        public object[] GetArgs()
        {
            return new object[] {
                TimeSpan.Zero,
                new TimeSpan(1,1,1,1,1),
                new TimeSpan(0,2,0,2,0),
                new TimeSpan(3,0,0,3,0),
                new TimeSpan(0,0,0,0,4),
                new TimeSpan(5,0,0,0,0),
            };
        }

        [Test]
        public void Test_Defaults()
        {
            var formats = new string[] {
                "{0}",
                "{1}",
                "{2}",
                "{3}",
                "{4}",
                "{5}",
            };
            var expected = new string[] {
                "less than 1 second",
                "1 day 1 hour 1 minute 1 second",
                "2 hours 2 seconds",
                "3 days 3 seconds",
                "less than 1 second",
                "5 days",
            };
            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);

        }

        [Test]
        public void Test_Options()
        {
            var formats = new string[] {
                "{0:noless}",
                "{1:hours}",
                "{1:hours minutes}",
                "{2:days milliseconds}",
                "{2:days milliseconds auto}",
                "{2:days milliseconds short}",
                "{2:days milliseconds fill}",
                "{2:days milliseconds full}",
                "{3:abbr}",
            };
            var expected = new string[] {
                "0 seconds",
                "25 hours",
                "25 hours 1 minute",
                "2 hours 2 seconds",
                "2 hours 2 seconds",
                "2 hours",
                "2 hours 0 minutes 2 seconds 0 milliseconds",
                "0 days 2 hours 0 minutes 2 seconds 0 milliseconds",
                "3d 3s",
            };
            var args = GetArgs();
            Smart.Default.Test(formats, args, expected);

        }
    }
}
