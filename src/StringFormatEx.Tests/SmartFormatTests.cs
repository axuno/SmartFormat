using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using NUnit.Framework;
using StringFormatEx.Core;

namespace StringFormatEx.Tests
{
    [TestFixture]
    public class SmartFormatTests
    {
        [Test]
        public void Test_Smart_Format()
        {
            var tests = new[]{
                new{ Title = "Test 0",
                     Format = "{0}",
                     Expected = "Zero",
                },
                new{ Title = "Test 1",
                     Format = "{1}",
                     Expected = "1",
                },
                new{ Title = "Test 2 - Format",
                     Format = "{2:N3}",
                     Expected = "2.000",
                },
                new{ Title = "Test 3 - Escaping",
                     Format = "{{{0}}} {{0}} {1:N0}}0}",
                     Expected = "{Zero} {0} N0}1",
                },
                new{ Title = "Test 4 - Array (no plugins)",
                     Format = "{4}",
                     Expected = "System.Char[]",
                },
                new{ Title = "Test 5 - Date format",
                     Format = "{5:ddd, MMMM d, yyyy}",
                     Expected = "Thu, May 5, 5555",
                },
                new{ Title = "Test 3",
                     Format = "{3}",
                     Expected = "333",
                },
            };
            // Create some testing arguments:
            var args = new object[] { "Zero", 1, 2, "333", "Four".ToCharArray(), new DateTime(5555, 5, 5, 5, 5, 5), new TimeSpan(6, 6, 6, 6) };

            // Assert all tests:
            tests.TryAll(t => 
                Assert.AreEqual(t.Expected, Smart.Format(t.Format, args), 
                "Result failed for \"{0}\".", t.Title)
            ).ThrowIfNotEmpty();
        }
    }
}
