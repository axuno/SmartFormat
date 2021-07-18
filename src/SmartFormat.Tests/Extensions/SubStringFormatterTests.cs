using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class SubStringFormatterTests
    {
        private readonly List<object> _people;
        private readonly SmartFormatter _smart;

        public SubStringFormatterTests()
        {
            _smart = Smart.CreateDefaultSmartFormat();
            _smart.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            _smart.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;

            if (_smart.FormatterExtensions.FirstOrDefault(fmt => fmt.Names.Contains("substr")) == null)
            {
                _smart.AddExtensions(new SubStringFormatter());
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
        public void LengthLongerThanString_ReturnEmptyString()
        {
            var formatter = _smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;
            
            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnEmptyString;
            Assert.AreEqual(string.Empty, _smart.Format("{Name:substr(0,999)}", _people.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ReturnStartIndexToEndOfString()
        {
            var formatter = _smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnStartIndexToEndOfString;
            Assert.AreEqual("Long John", _smart.Format("{Name:substr(0,999)}", _people.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ThrowException()
        {
            var formatter = _smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ThrowException;
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => _smart.Format("{Name:substr(0,999)}", _people.First()));

            formatter.OutOfRangeBehavior = behavior;
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
            Assert.AreEqual(new SubStringFormatter().NullDisplayString, _smart.Format("{Name:substr(0,3)}", new Dictionary<string, string?> { { "Name", null } }));
        }
    }
}