using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class SubStringFormatterTests
    {
        private List<object> _people;
        private SmartFormatter _smart;

        public SubStringFormatterTests()
        {
            _smart = Smart.CreateDefaultSmartFormat();
            _smart.Settings.FormatErrorAction = ErrorAction.ThrowError;
            _smart.Settings.ParseErrorAction = ErrorAction.ThrowError;

            if (_smart.FormatterExtensions.FirstOrDefault(fmt => fmt.Names.Contains("substr")) == null)
            {
                _smart.FormatterExtensions.Add(new SubStringFormatter());
            }

            _people = new List<object>
                {new {Name = "Long John", City = "New York"}, new {Name = "Short Mary", City = "Massachusetts"},};
        }


        [Test]
        public void NoParameters()
        {
            Assert.AreEqual("No parentheses: Long John", _smart.Format("No parentheses: {Name:substr}", _people.First()));
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => _smart.Format("No parameters: {Name:substr()}", _people.First()));
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => _smart.Format("Only delimiter: {Name:substr(,)}", _people.First()));
        }

        [Test]
        public void StartPositionLongerThanString()
        {
            Assert.AreEqual(string.Empty, _smart.Format("{Name:substr(999)}", _people.First()));
        }

        [Test]
        public void StartPositionAndLengthLongerThanString()
        {
            Assert.AreEqual(string.Empty, _smart.Format("{Name:substr(999,1)}", _people.First()));
        }

        [Test]
        public void OnlyPositiveStartPosition()
        {
            Assert.AreEqual("John", _smart.Format("{Name:substr(5)}", _people.First()));
        }

        [Test]
        public void StartPositionAndPositiveLength()
        {
            Assert.AreEqual("New", _smart.Format("{City:substr(0,3)}", _people.First()));
        }

        [Test]
        public void OnlyNegativeStartPosition()
        {
            Assert.AreEqual("John", _smart.Format("{Name:substr(-4)}", _people.First()));
        }

        [Test]
        public void NegativeStartPositionAndPositiveLength()
        {
            Assert.AreEqual("Jo", _smart.Format("{Name:substr(-4, 2)}", _people.First()));
        }

        [Test]
        public void NegativeStartPositionAndNegativeLength()
        {
            Assert.AreEqual("Joh", _smart.Format("{Name:substr(-4, -1)}", _people.First()));
        }

        [Test]
        public void DataItemIsNull()
        {
            Assert.AreEqual(new SubStringFormatter().NullDisplayString, _smart.Format("{Name:substr(0,3)}", new Dictionary<string, string> { { "Name", null } }));
        }
    }
}