using NUnit.Framework;

namespace SmartFormat.Tests.Core
{
    [TestFixture]
    public class NestingTests
    {
        public NestingTests()
        {
            this.data = new {
                One = 1,
                ChildOne = new {
                    Two = 2,
                    ChildTwo = new {
                        Three = 3,
                        ChildThree = new {
                            Four = 4,
                        }
                    }
                }
            };
        }
        private object data;

        [Test]
        [TestCase("{ChildOne.ChildTwo: {Three} {0.One} }", " 3 1 ")]
        [TestCase("{ChildOne.ChildTwo.ChildThree: {Four} {0.ChildOne: {Two} {0.One} } }", " 4  2 1  ")]
        public void Nesting_can_access_root_via_number(string format, string expectedOutput)
        {
            var actual = Smart.Format(format, data);
            Assert.AreEqual(expectedOutput, actual);
        }

        [Test]
        [TestCase("{ChildOne.ChildTwo.ChildThree: {Four} {One} }", " 4 1 ")]
        [TestCase("{ChildOne: {ChildTwo: {ChildThree: {Four} {Three} {Two} {One} } } }", "   4 3 2 1   ")]
        [TestCase("{ChildOne: {ChildTwo: {ChildThree: {Four} {ChildTwo.Three} {ChildOne.Two} {One} } } }", "   4 3 2 1   ")]
        [TestCase("{ChildOne: {ChildTwo: {ChildThree: {ChildOne: {ChildTwo: {ChildThree: {Four} } } } } } }", "      4      ")]
        public void Nesting_can_access_outer_scopes(string format, string expectedOutput)
        {
            var actual = Smart.Format(format, data);
            Assert.AreEqual(expectedOutput, actual);
        }

        [Test]
        [TestCase("{ChildOne.ChildTwo.ChildThree:{Four}{One}}","41")]
        [TestCase("{ChildOne:{ChildTwo:{ChildThree:{Four}{Three}{Two}{One}}}}","4321")]
        [TestCase("{ChildOne:{ChildTwo:{ChildThree:{Four}{ChildTwo.Three}{ChildOne.Two}{One}}}}","4321")]
        [TestCase("{ChildOne:{ChildTwo:{ChildThree:{ChildOne:{ChildTwo:{ChildThree:{Four}}}}}}}","4")]
        public void Nesting_can_access_outer_scopes_no_blanks(string format, string expectedOutput)
        {
            // Removing the spaces from Nesting_can_access_outer_scopes requires alternative escaping of { and }!
            var sf = Smart.CreateDefaultSmartFormat();
            sf.Parser.UseAlternativeEscapeChar('\\');
            var actual = sf.Format(format, data);
            Assert.AreEqual(expectedOutput, actual);
        }
    }
}
