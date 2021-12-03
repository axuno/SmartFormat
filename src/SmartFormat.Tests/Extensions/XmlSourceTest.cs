using System.Xml.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class XmlSourceTest
    {
        public const string TwoLevelXml = "<root>" +
                                      "<Person>" +
                                      "  <FirstName>Joe</FirstName>" +
                                      "  <LastName>Doe</LastName>" +
                                      "  <Phone>123-123-1234</Phone>" +
                                      "</Person>" +
                                      "<Person>" +
                                      "  <FirstName>Jack</FirstName>" +
                                      "  <LastName>Doe</LastName>" +
                                      "  <Phone>789-789-7890</Phone>" +
                                      "</Person>" +
                                      "</root>";

        private const string OneLevelXml = "<root>" +
                                           "<FirstName>Joe</FirstName>" +
                                           "<LastName>Doe</LastName>" +
                                           "<Dob>1950-05-05</Dob>" +
                                           "</root>";
        private const string OneLevelXmlWithNameSpaces = "<my:root xmlns:my='http://tempuri.org'>" +
                                                   "<my:FirstName>Joe</my:FirstName>" +
                                                   "<my:LastName>Doe</my:LastName>" +
                                                   "<my:Dob>1950-05-05</my:Dob>" +
                                                   "</my:root>";
        private const string XmlMultipleFirstNameStr = "<root>" +
                                                       "<FirstName>Joe</FirstName>" +
                                                       "<FirstName>Jack</FirstName>" +
                                                       "<LastName>Doe</LastName>" +
                                                       "<FirstName>Jim</FirstName>" +
                                                       "</root>";

        [Test]
        public void Format_SingleLevelXml_Replaced()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(OneLevelXml);
            // act
            var res = smart.Format("Mr. {FirstName:xml:} {LastName:xml:}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe Doe", res);
        }

        [Test]
        public void Format_SingleLevelXml_CanAccessWithIndex0()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(OneLevelXml);
            // act
            var res = smart.Format("Mr. {FirstName.0:xml:}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe", res);
        }

        [Test]
        public void Format_XmlWithNamespaces_IgnoringNamespace()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(OneLevelXmlWithNameSpaces);
            // act
            var res = smart.Format("Mr. {FirstName:xml:} {LastName:xml:}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe Doe", res);
        }

        [Test]
        public void Format_SingleLevelXml_TemplateWithCurlyBraces_Escaped()
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {StringFormatCompatibility = false});
            // arrange
            var xmlEl = XElement.Parse(OneLevelXml);
            // act
            var res = smart.Format("Mr. \\{{LastName:xml:}\\}", xmlEl);
            // assert
            Assert.AreEqual("Mr. {Doe}", res);
        }

        [Test]
        public void Format_MultipleElement_AccessibleByIndex()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(XmlMultipleFirstNameStr);
            // act
            var res = smart.Format("Mr. {FirstName.1:xml:} {LastName:xml:}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Jack Doe", res);
        }

        [Test]
        public void Format_MultipleElement_WithoutIndexesReturnsFirst()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(XmlMultipleFirstNameStr);
            // act
            var res = smart.Format("Mr. {FirstName:xml:}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe", res);
        }

        [Test]
        public void Format_MultipleElement_FormatsCount()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(XmlMultipleFirstNameStr);
            // act
            var res = smart.Format("There{FirstName.Count:cond: is {} Doe | are {} Does}", xmlEl);
            // assert
            Assert.AreEqual("There are 3 Does", res);
        }

        [Test]
        public void Format_MultipleElement_FormatsAsList()
        {
            // arrange
            var smart = Smart.CreateDefaultSmartFormat();
            var xmlEl = XElement.Parse(XmlMultipleFirstNameStr);
            // act
            var res = smart.Format("There are{FirstName:list: {}|,|, and} Doe", xmlEl);
            // assert
            Assert.AreEqual("There are Joe, Jack, and Jim Doe", res);
        }

        [Test]
        public void Format_TwoLevelXml_InvalidSelectors_Throws()
        {
            var smart = Smart.CreateDefaultSmartFormat(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
            // arrange
            var xmlEl = XElement.Parse(TwoLevelXml);
            // act
            Assert.Throws<FormattingException>(() => smart.Format("{SomethingNonExisting}{EvenMore}", xmlEl));
        }
    }
}
