using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.OldTests
{
    public struct Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width;
        public int Height;
    }

    /// <summary>
    /// DANGER ZONE: ConditionalFormatter and PluralLocalizationFormatter use similar syntax.
    /// The loading sequence of these formatters, this may lead to surprising test results.
    /// ALWAYS choose explicitly, which formatter to use, i.e. {0:cond:item|items} or {0:p:item|items}
    /// </summary>
    [TestFixture]
    public class CodeProjectExampleTests
    {
        private readonly SmartFormatter _smart;

        public CodeProjectExampleTests()
        {
            _smart = CreateTestFormatter();
        }

        #region: Create formatter :

        private SmartFormatter CreateTestFormatter()
        {
            // Register all default extensions here:
            var formatter = new SmartFormatter();
            // Add all extensions:
            // Note, the order is important; the extensions
            // will be executed in this order:

            var listFormatter = new ListFormatter(formatter);

            // sources for specific types must be in the list before ReflectionSource
            formatter.AddExtensions(
                (ISource) listFormatter, // ListFormatter MUST be first
                new DictionarySource(formatter),
                new ValueTupleSource(formatter),
                new JsonSource(formatter),
                new XmlSource(formatter),
                new ReflectionSource(formatter),

                // The DefaultSource reproduces the string.Format behavior:
                new DefaultSource(formatter)
            );
            formatter.AddExtensions(
                (IFormatter) listFormatter,
                new PluralLocalizationFormatter("en"),
                new ConditionalFormatter(),
                new TimeFormatter("en"),
                new XElementFormatter(),
                new ChooseFormatter(),
                new SubStringFormatter(),
                new DefaultFormatter()
            );

            return formatter;
        }

        #endregion

        private Person MakeQuentin()
        {
            var p = new Person()
            {
                FullName = "Quentin Starin",
                Birthday = DateTime.Today.AddYears(-30),
                Address = new Address("101 1st Ave", "Minneapolis", States.Minnesota, "55401"),
                Friends = new List<Person>
                    {
                        new Person() { FullName = "John Smith", Birthday = new DateTime(1978, 1, 1) },
                        new Person() { FullName = "Bob Johnson", Birthday = new DateTime(1957, 1, 1) },
                        new Person() { FullName = "Mary Meyer", Birthday = new DateTime(1990, 1, 1) },
                        new Person() { FullName = "Dr. Jamal Cornucopia", Birthday = new DateTime(1973, 1, 1) }
                    }
            };

            return p;
        }

        private Person MakeMelinda()
        {
            var p = new Person() {
                                     FirstName = "Melinda"
                                 };

            return p;
        }


        [Test]
        public void BasicSyntax()
        {
            var p = MakeQuentin();

            var formatString = "{0} is {1} years old and has {2:N2} friends.";
            var expectedOutput = "Quentin is 30 years old and has 4.00 friends.";

            var specificCulture = new CultureInfo("en-us");
            string actualOutput = _smart.Format(specificCulture, formatString, p.FirstName, p.Age, p.Friends.Count);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void BasicReflection()
        {
            var p = MakeQuentin();

            var formatString = "{FirstName} is {Age} years old and has {Friends.Count:N2} friends.";
            var expectedOutput = "Quentin is 30 years old and has 4.00 friends.";

            var specificCulture = new CultureInfo("en-us");
            string actualOutput = _smart.Format(specificCulture, formatString, p);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void BasicConditional()
        {

            var formatString = "There {0:is|are} {0} item{0:|s} remaining...";
            var expectedOutput = "There are 5 items remaining...";

            string actualOutput = _smart.Format(formatString, 5);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void BasicArray()
        {
            var data = new DateTime[]
                {
                    new DateTime(1999, 12, 31),
                    new DateTime(2010, 10, 10),
                    new DateTime(3000, 1, 1),
                };

            var formatString = "All dates: {0:{:M/d/yyyy}| and }.";
            var expectedOutput = "All dates: 12/31/1999 and 10/10/2010 and 1/1/3000.";

            var specificCulture = new CultureInfo("en-us");
            string actualOutput = _smart.Format(specificCulture, formatString, data);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ArgumentIndexes()
        {
            var p1 = MakeQuentin();
            var p2 = MakeMelinda();

            var formatString = "Person0: {0.FirstName}, Person1: {1.FirstName}";
            var expectedOutput = "Person0: Quentin, Person1: Melinda";

            string actualOutput = _smart.Format(formatString, p1, p2);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void DotNotation()
        {
            var p1 = MakeQuentin();

            var formatString = "{Address.City}, {Address.State} {Address.Zip}";
            var expectedOutput = "Minneapolis, Minnesota 55401";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void DotNotation_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;

            var p1 = MakeQuentin();

            var formatString = "{aDDress.ciTy}, {Address.sTate} {Address.ZiP}";
            var expectedOutput = "Minneapolis, Minnesota 55401";

            string actualOutput = formatter.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void Nesting()
        {
            var p1 = MakeQuentin();

            var formatString = "{Address:{City}, {State} {Zip}}";
            var expectedOutput = "Minneapolis, Minnesota 55401";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void NestingCollection()
        {
            Size[] sizes = {new Size(1, 1), new Size(4, 3), new Size(16, 9)};

            var formatString = "{0:({Width} x {Height})| and }";
            var expectedOutput = "(1 x 1) and (4 x 3) and (16 x 9)";

            string actualOutput = _smart.Format(formatString, sizes);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void NestingCollection_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;

            Size[] sizes = { new Size(1, 1), new Size(4, 3), new Size(16, 9) };

            var formatString = "{0:({widTh} x {hEIght})| and }";
            var expectedOutput = "(1 x 1) and (4 x 3) and (16 x 9)";

            string actualOutput = formatter.Format(formatString, sizes);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void Scope()
        {
            var p1 = MakeQuentin();

            var formatString = "{0.Address:{City}, {State} {Zip}}";
            var expectedOutput = "Minneapolis, Minnesota 55401";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ScopeOmitFirstArgIndex()
        {
            var p1 = MakeQuentin();
            var p2 = MakeMelinda();

            var formatString = "{FirstName} {0.FirstName} {1.FirstName}";
            var expectedOutput = "Quentin Quentin Melinda";

            string actualOutput = _smart.Format(formatString, p1, p2);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ScopeEmptyPlaceholder()
        {
            var p1 = MakeQuentin();

            var formatString = "{Friends.Count:There {:is|are} {} friend{:|s}.}";
            var expectedOutput = "There are 4 friends.";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void OutOfScope()
        {
            var p1 = MakeQuentin();

            var formatString = "{Address:{City}, {State}, {0.Age} {0.FullName}}";
            var expectedOutput = "Minneapolis, Minnesota, 30 Quentin Starin";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void PluralNumber_OneOrDefault_Value0()
        {
            var formatString = "{0} {0:p:item|items}";
            var expectedOutput = "0 items";

            string actualOutput = _smart.Format(formatString, 0);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void PluralNumber_OneOrDefault_Value1()
        {
            var formatString = "{0} {0:p:item|items}";
            var expectedOutput = "1 item";

            string actualOutput = _smart.Format(formatString, 1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ConditonalNumber_OneOrDefault_Value3()
        {
            var formatString = "{0} {0:cond:item|items}";
            var expectedOutput = "3 items";

            string actualOutput = _smart.Format(formatString, 3);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ConditonalNumber_ZeroOneOrDefault_Value0()
        {
            var formatString = "{0:cond:no item|one item|many items}";
            var expectedOutput = "no item";

            string actualOutput = _smart.Format(formatString, 0);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ConditonalNumber_ZeroOneOrDefault_Value1()
        {
            var formatString = "{0:cond:no item|one item|many items}";
            var expectedOutput = "one item";

            string actualOutput = _smart.Format(formatString, 1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ConditonalNumber_ZeroOneOrDefault_Value3()
        {
            var formatString = "{0:cond:no item|one item|many items}";
            var expectedOutput = "many items";

            string actualOutput = _smart.Format(formatString, 3);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void PluralNumber_NegativeZeroOneOrDefault_Value0()
        {
            var formatString = "{0:p:negative items|no item|one item|many items}";
            var expectedOutput = "no item";

            string actualOutput = _smart.Format(formatString, 0);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void PluralNumber_NegativeZeroOneOrDefault_Value1()
        {
            var formatString = "{0:p:negative items|no item|one item|many items}";
            var expectedOutput = "one item";

            string actualOutput = _smart.Format(formatString, 1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void PluralNumber_NegativeZeroOneOrDefault_Value4()
        {
            var formatString = "{0:p:negative items|no item|one item|many items}";
            var expectedOutput = "many items";

            string actualOutput = _smart.Format(formatString, 4);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void PluralNumber_NegativeZeroOneOrDefault_ValueNegative2()
        {
            var formatString = "{0:p:negative items|no item|one item|many items}";
            var expectedOutput = "negative items";

            string actualOutput = _smart.Format(formatString, -2);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ComplexCondition()
        {
            var p1 = MakeQuentin();

            var formatString = "{Age::>=55?Senior Citizen|>=30?Adult|>=18?Young Adult|>12?Teenager|>2?Child|Baby}";
            var expectedOutput = "Adult";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ComplexConditionFirstMatchingCase()
        {
            var p1 = new Person() { Birthday = DateTime.MinValue };

            var formatString = "{Age::>=55?Senior Citizen|>=30?Adult|>=18?Young Adult|>12?Teenager|>2?Child|Baby}";
            var expectedOutput = "Senior Citizen";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ComplexConditionFirstMatchingCase_CaseInsensitive()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;

            var p1 = new Person() { Birthday = DateTime.MinValue };

            var formatString = "{aGe::>=55?Senior Citizen|>=30?Adult|>=18?Young Adult|>12?Teenager|>2?Child|Baby}";
            var expectedOutput = "Senior Citizen";

            string actualOutput = formatter.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void ComplexConditionFallThroughCase()
        {
            var p1 = new Person() { Birthday = DateTime.Today };

            var formatString = "{Age::>=55?Senior Citizen|>=30?Adult|>=18?Young Adult|>12?Teenager|>2?Child|Baby}";
            var expectedOutput = "Baby";

            string actualOutput = _smart.Format(formatString, p1);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void AdvancedArray()
        {
            var data = new DateTime[] {
                                          new DateTime(1999, 12, 31),
                                          new DateTime(2010, 10, 10),
                                          new DateTime(3000, 1, 1),
                                      };

            var formatString = "{0:{:M/d/yyyy}|, |, and }.";
            var expectedOutput = "12/31/1999, 10/10/2010, and 1/1/3000.";

            var specificCulture = new CultureInfo("en-us");
            string actualOutput = _smart.Format(specificCulture, formatString, data);
            Assert.AreEqual(expectedOutput, actualOutput);
        }


        [Test]
        public void AdvancedArrayUsingIndex()
        {
            var data = new string[] { "A", "B", "C" };

            var formatString = "{0:{} = {Index}|, }";
            var expectedOutput = "A = 0, B = 1, C = 2";

            //string actualOutput = _smart.Format(formatString, data);
            string actualOutput = _smart.Format(formatString, (object)data);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void EscapingDoubleBraces()
        {
            var args = new object[] { "Zero", "One", "Two", "Three" };

            var format = "{0} {{0}} {{{0}}}";
            var expected = "Zero {0} {Zero}";

            var actual = Smart.CreateDefaultSmartFormat().Format(format, args);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void EscapingSlashBraces()
        {
            var smart = new SmartFormatter();
            smart.Parser.UseAlternativeEscapeChar('\\');
            smart.AddExtensions(new DefaultFormatter());
            smart.AddExtensions(new DefaultSource(smart));

            var args = new object[] { "Zero", "One", "Two", 3 };

            var format = @"{0} \{0\} \{{0}\} {3:00\}} {3:00}\}";
            var expected = "Zero {0} {Zero} 03} 03}";

            var actual = _smart.Format(format, args);
            Assert.AreEqual(expected, actual);
        }
    }
}
