using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    class FormatCacheTests
    {
        [Test]
        public void Create_Cache()
        {
            var sf = new SmartFormatter();
            var format = new Format(sf.Settings, "the base string");
            var fc = new FormatCache(format);
            Assert.AreEqual(format, fc.Format);
            Assert.IsAssignableFrom<Dictionary<string,object>>(fc.CachedObjects);
            fc.CachedObjects.Add("key", "value");
            Assert.IsTrue(fc.CachedObjects["key"].ToString() == "value");
        }
    }
}
