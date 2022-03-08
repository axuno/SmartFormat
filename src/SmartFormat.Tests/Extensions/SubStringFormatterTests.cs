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
        private readonly object _person = new {Name = "Long John", City = "New York"};

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

        [Test]
        public void NoParentheses_Should_Work()
        {
            var smart = GetFormatter();
            Assert.AreEqual("No parentheses: Long John", smart.Format("No parentheses: {Name:substr}", _person));
        }

        [Test]
        public void NoParameters_Should_Throw()
        {
            var smart = GetFormatter();
            Assert.Throws<FormattingException>(() => smart.Format("Only delimiter: {Name:substr(,)}", _person));
        }

        [Test]
        public void OnlyDelimiter_Should_Throw()
        {
            var smart = GetFormatter();
            Assert.Throws<FormattingException>(() => smart.Format("Only delimiter: {Name:substr(,)}", _person));
        }

        [Test]
        public void StartPositionLongerThanString()
        {
            var smart = GetFormatter();
            Assert.AreEqual(string.Empty, smart.Format("{Name:substr(999)}", _person));
        }

        [Test]
        public void StartPositionAndLengthLongerThanString()
        {
            var smart = GetFormatter();
            Assert.AreEqual(string.Empty, smart.Format("{Name:substr(999,1)}", _person));
        }

        [Test]
        public void LengthLongerThanString_ReturnEmptyString()
        {
            var smart = GetFormatter();
            var formatter = smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;
            
            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnEmptyString;
            Assert.AreEqual(string.Empty, smart.Format("{Name:substr(0,999)}", _person));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ReturnStartIndexToEndOfString()
        {
            var smart = GetFormatter();
            var formatter = smart.GetFormatterExtension<SubStringFormatter>()!;
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnStartIndexToEndOfString;
            Assert.AreEqual("Long John", smart.Format("{Name:substr(0,999)}", _person));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ThrowException()
        {
            var smart = GetFormatter();
            var formatter = smart.GetFormatterExtension<SubStringFormatter>()!;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ThrowException;
            Assert.Throws<FormattingException>(() => smart.Format("{Name:substr(0,999)}", _person));
        }

        [Test]
        public void OnlyPositiveStartPosition()
        {
            var smart = GetFormatter();
            Assert.AreEqual("John", smart.Format("{Name:substr(5)}", _person));
        }

        [Test]
        public void StartPositionAndPositiveLength()
        {
            var smart = GetFormatter();
            Assert.AreEqual("New", smart.Format("{City:substr(0,3)}", _person));
        }

        [Test]
        public void OnlyNegativeStartPosition()
        {
            var smart = GetFormatter();
            Assert.AreEqual("John", smart.Format("{Name:substr(-4)}", _person));
        }

        [Test]
        public void NegativeStartPositionAndPositiveLength()
        {
            var smart = GetFormatter();
            Assert.AreEqual("Jo", smart.Format("{Name:substr(-4, 2)}", _person));
        }

        [Test]
        public void NegativeStartPositionAndNegativeLength()
        {
            var smart = GetFormatter();
            Assert.AreEqual("Joh", smart.Format("{Name:substr(-4, -1)}", _person));
        }

        [Test]
        public void DataItemIsNull()
        {
            var smart = GetFormatter();
            var ssf = smart.GetFormatterExtension<SubStringFormatter>();
            ssf!.NullDisplayString = "???";
            var result = smart.Format("{Name:substr(0,3)}", new KeyValuePair<string, object?>("Name", null));
            Assert.That(result, Is.EqualTo(ssf.NullDisplayString));
        }

        [Test]
        public void DataItemIsNull_With_ChildFormat()
        {
            var smart = GetFormatter();
            var ssf = smart.GetFormatterExtension<SubStringFormatter>()!.NullDisplayString = "???";
            // If a nested format is used, it gets NULL, too.
            // Then, NullDisplayString will not be output
            var result = smart.Format("{Name:substr(0,3):{:isnull:It is null}}", new KeyValuePair<string, object?>("Name", null));            
            Assert.That(result, Is.EqualTo("It is null"));
        }

        [Test]
        public void Test_With_Changed_SplitChar()
        {
            var smart = GetFormatter();
            var currentSplitChar = smart.GetFormatterExtension<SubStringFormatter>()!.SplitChar;
            // Change SplitChar from default ',' to '|'
            smart.GetFormatterExtension<SubStringFormatter>()!.SplitChar = '|';
            Assert.AreEqual("Joh", smart.Format("{Name:substr(-4|-1)}", _person));
            Assert.That(currentSplitChar, Is.EqualTo(',')); // make sure there was a change
        }

        [Test]
        public void NamedFormatterWithoutOptionsShouldThrow()
        {
            var smart = GetFormatter();
            Assert.That(() => smart.Format("{Name:substr()}", _person), Throws.Exception.TypeOf<FormattingException>());
        }

        [Test]
        public void FormatterWithoutStringArgumentShouldThrow()
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

        [Test]
        public void Format_Without_Nesting_Should_Throw()
        {
            var smart = GetFormatter();
            Assert.That(() => smart.Format("{0:substr(0,2):just text}", "input"), Throws.Exception.TypeOf<FormattingException>());
        }

        [Test]
        public void SubString_Using_Simple_Format()
        {
            var smart = GetFormatter();
            var result = smart.Format("{0:substr(0,2):{ToLower}}", "ABC");
            Assert.That(result, Is.EqualTo("ab"));
        }

        [Test]
        public void SubString_Using_Complex_Format()
        {
            var smart = GetFormatter();
            var result = smart.Format("{0:substr(0,2):{ToLower.ToCharArray:list:'{}'| and }}", "ABC");
            Assert.That(result, Is.EqualTo("'a' and 'b'"));
        }
    }
}
