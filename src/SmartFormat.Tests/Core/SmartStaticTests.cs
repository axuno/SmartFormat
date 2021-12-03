using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.Core
{
    /// <summary>
    /// Unit test must never use the static versions of <see cref="Smart"/>.
    /// This would make tests not repeatable.
    /// </summary>
    [TestFixture]
    public class SmartStaticTests
    {
        [Test]
        public void Smart_Format_One_Arg()
        {
            Assert.That(Smart.Format("{0}", "SMART"), Is.EqualTo("SMART"));
        }

        [Test]
        public void Smart_Format_Two_Args()
        {
            Assert.That(Smart.Format("{0} {1}", "VERY","SMART"), Is.EqualTo("VERY SMART"));
        }

        [Test]
        public void Smart_Format_Three_Args()
        {
            Assert.That(Smart.Format("{0} {1} {2}", "THIS","IS","SMART"), Is.EqualTo("THIS IS SMART"));
        }

        [Test]
        public void Smart_Format_One_Null_Arg()
        {
            Assert.That(Smart.Format("SMART{0}", (object?) null), Is.EqualTo("SMART"));
        }

        [Test]
        public void Smart_Format_Two_Null_Args()
        {
            Assert.That(Smart.Format("VERY{0} {1}SMART", null, null), Is.EqualTo("VERY SMART"));
        }

        [Test]
        public void Smart_Format_Three_Null_Args()
        {
            Assert.That(Smart.Format("THIS{0} IS {1}SMART{2}", null, null, null), Is.EqualTo("THIS IS SMART"));
        }

        [Test]
        public void Smart_Format_Param_Null_Args()
        {
            Assert.That(Smart.Format("THIS{0} IS {1}SMART{2}{3}", null, null, null, null), Is.EqualTo("THIS IS SMART"));
        }

        [Test]
        public void Smart_Format_With_FormatProvider()
        {
            Assert.That(Smart.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", "This","is","culture"), Is.EqualTo("This is culture"));
        }

        [Test]
        public void Smart_Default()
        {
            Smart.Default = new SmartFormatter(new SmartSettings {StringFormatCompatibility = !new SmartSettings().StringFormatCompatibility});
            Assert.That(Smart.Default.Settings.StringFormatCompatibility, Is.EqualTo(!new SmartSettings().StringFormatCompatibility));
            Smart.Default = Smart.CreateDefaultSmartFormat(); // reset
        }
    }
}
