using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Formatting;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class SubStringFormatterTests
    {
        private readonly List<object> _people;

        private static SmartFormatter GetFormatter()
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
            {
                Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError},
                Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError}
            });

            if (smart.FormatterExtensions.FirstOrDefault(fmt => fmt.Name.Equals("substr")) == null)
            {
                smart.AddExtensions(new SubStringFormatter());
            }

            return smart;
        }

        public SubStringFormatterTests()
        {
            _people = new List<object>
                {new {Name = "Long John", City = "New York"}, new {Name = "Short Mary", City = "Massachusetts"},};
        }

        [Test]
        public void NoParameters()
        {
            var smart = GetFormatter();
            Assert.AreEqual("No parentheses: Long John", smart.Format("No parentheses: {Name:substr}", _people.First()));
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => smart.Format("No parameters: {Name:substr()}", _people.First()));
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => smart.Format("Only delimiter: {Name:substr(,)}", _people.First()));
        }

        [Test]
        public void StartPositionLongerThanString()
        {
            var smart = GetFormatter();
            Assert.AreEqual(string.Empty, smart.Format("{Name:substr(999)}", _people.First()));
        }

        [Test]
        public void StartPositionAndLengthLongerThanString()
        {
            var smart = GetFormatter();
            Assert.AreEqual(string.Empty, smart.Format("{Name:substr(999,1)}", _people.First()));
        }

        [Test]
        public void LengthLongerThanString_ReturnEmptyString()
        {
            var smart = GetFormatter();
            var formatter = smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;
            
            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnEmptyString;
            Assert.AreEqual(string.Empty, smart.Format("{Name:substr(0,999)}", _people.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ReturnStartIndexToEndOfString()
        {
            var smart = GetFormatter();
            var formatter = smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnStartIndexToEndOfString;
            Assert.AreEqual("Long John", smart.Format("{Name:substr(0,999)}", _people.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ThrowException()
        {
            var smart = GetFormatter();
            var formatter = smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ThrowException;
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => smart.Format("{Name:substr(0,999)}", _people.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void OnlyPositiveStartPosition()
        {
            var smart = GetFormatter();
            Assert.AreEqual("John", smart.Format("{Name:substr(5)}", _people.First()));
        }

        [Test]
        public void StartPositionAndPositiveLength()
        {
            var smart = GetFormatter();
            Assert.AreEqual("New", smart.Format("{City:substr(0,3)}", _people.First()));
        }

        [Test]
        public void OnlyNegativeStartPosition()
        {
            var smart = GetFormatter();
            Assert.AreEqual("John", smart.Format("{Name:substr(-4)}", _people.First()));
        }

        [Test]
        public void NegativeStartPositionAndPositiveLength()
        {
            var smart = GetFormatter();
            Assert.AreEqual("Jo", smart.Format("{Name:substr(-4, 2)}", _people.First()));
        }

        [Test]
        public void NegativeStartPositionAndNegativeLength()
        {
            var smart = GetFormatter();
            Assert.AreEqual("Joh", smart.Format("{Name:substr(-4, -1)}", _people.First()));
        }

        [Test]
        public void DataItemIsNull()
        {
            var smart = GetFormatter();
            var ssf = smart.GetFormatterExtension<SubStringFormatter>();
            ssf!.NullDisplayString = "???";
            ssf!.ParameterDelimiter = '*';
            Assert.AreEqual(ssf.NullDisplayString, smart.Format("{Name:substr(0*3)}", new Dictionary<string, string?> { { "Name", null } }));
        }

        [Test]
        public void NamedFormatterWithoutOptionsShouldThrow()
        {
            var smart = GetFormatter();
            Assert.That(() => smart.Format("{Name:substr()}", _people.First()), Throws.Exception.TypeOf<FormattingException>());
        }

        [Test]
        public void NamedFormatterWithoutStringArgumentShouldThrow()
        {
            var smart = GetFormatter();
            Assert.That(() => smart.Format("{0:substr(0,2)}", new object()), Throws.Exception.TypeOf<FormattingException>());
        }

        [Test]
        public void ImplicitFormatterEvaluation_With_Wrong_Args_Should_Fail()
        {
            var smart = GetFormatter();
            Assert.That(
                smart.GetFormatterExtension<SubStringFormatter>()!.TryEvaluateFormat(
                    FormattingInfoExtensions.Create("{0::(0,2)}", new List<object?>(new[] {new object()}))), Is.EqualTo(false));
        }
    }
}