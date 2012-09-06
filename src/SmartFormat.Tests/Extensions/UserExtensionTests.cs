using System;
using System.Globalization;
using NUnit.Framework;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class UserExtensionTests
    {
        [TearDown]
        public void TearDown()
        {
            Smart.Default.ClearUserExtensions();
        }

        [Test]
        public void Test_Custom_Formatter()
        {
            //arrange
            var guid = new Guid("4f86ca67-94c2-4ce1-b79d-c4ed063d3a8e");
            Smart.Default.AddUserExtensions(new CustomGuidFormatter());

            //act
            var formatted = Smart.Format(CultureInfo.InvariantCulture, "{0}, {1}", guid, new DateTime(2012, 08, 24));

            Assert.That(formatted, Is.EqualTo("4f86ca..., 08/24/2012 00:00:00"));
        }

        [Test]
        public void Test_Clear_Custom_Formatters()
        {
            //arrange
            var guid = new Guid("4f86ca67-94c2-4ce1-b79d-c4ed063d3a8e");
            Smart.Default.AddUserExtensions(new CustomGuidFormatter());
            Smart.Default.ClearUserExtensions();

            //act
            var formatted = Smart.Format(CultureInfo.InvariantCulture, "{0}, {1}", guid, new DateTime(2012, 08, 24));

            Assert.That(formatted, Is.EqualTo("4f86ca67-94c2-4ce1-b79d-c4ed063d3a8e, 08/24/2012 00:00:00"));
        }
    }

    class CustomGuidFormatter : IFormatter
    {
        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            if (!(current is Guid))
                return;

            var guid = (Guid)current;

            output.Write(guid.ToString().Substring(0, 6), formatDetails);
            output.Write("...", formatDetails);

            handled = true;
        }
    }
}
