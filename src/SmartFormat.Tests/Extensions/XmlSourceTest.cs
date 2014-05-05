using System.Xml.Linq;
using NUnit.Framework;
using SmartFormat.Core;
using SmartFormat.Extensions;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class XmlSourceTest
    {
        private const string TwoLevelXml = "<root>" +
                                      "<Person>" +
                                      "  <FirstName>Joe</FirstName>" +
                                      "  <LastName>Doe</LastName>" +
                                      "</Person>" +
                                      "<Phone>123-123-1234</Phone>" +
                                      "</root>";

        private const string OneLevelXml = "<root>" +
                                           "<FirstName>Joe</FirstName>" +
                                           "<LastName>Doe</LastName>" +
                                           "<Dob>1950-05-05</Dob>" +
                                           "</root>";

        [Test]
        public void Format_SingleLevelXml_Replaced()
        {
            // arrange
            var xmlEl = XElement.Parse(OneLevelXml);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            var res = Smart.Format("Mr. {FirstName} {LastName}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe Doe", res);
        }

        [Test]
        public void Format_SingleLevelXml_TemplateWithCurlyBraces_Escaped()
        {
            // arrange
            var xmlEl = XElement.Parse(OneLevelXml);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            var res = Smart.Format("Mr. {{{LastName}}}", xmlEl);
            // assert
            Assert.AreEqual("Mr. {Doe}", res);
        }

        [Test]
        public void Format_DuplicateElement_ReplacedWithFirst()
        {
            // arrange
            const string xmlStr = "<root>" +
                                  "<FirstName>Joe</FirstName>" +
                                  "<FirstName>Jack</FirstName>" +
                                  "<LastName>Doe</LastName>" +
                                  "<FirstName>Jim</FirstName>" +
                                  "</root>";
            var xmlEl = XElement.Parse(xmlStr);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            var res = Smart.Format("Mr. {FirstName} {LastName}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe Doe", res);
        }

        [Test]
        public void Format_TwoLevelXml_TwoLevelSelectors_Replaced()
        {
            // arrange
            var xmlEl = XElement.Parse(TwoLevelXml);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            var res = Smart.Format("Mr. {Person.FirstName} {Person.LastName}, {Phone}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe Doe, 123-123-1234", res);
        }

        [Test]
        public void Format_TwoLevelXml_OneLevelSelector_Replaced()
        {
            // arrange
            var xmlEl = XElement.Parse(TwoLevelXml);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            var res = Smart.Format("Mr. {Person}", xmlEl);
            // assert
            Assert.AreEqual("Mr. JoeDoe", res);
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void Format_TwoLevelXml_InvalidSelectors_Throws()
        {
            // arrange
            var xmlEl = XElement.Parse(TwoLevelXml);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            Smart.Format("{SomethingNonExisting}{EvenMore}", xmlEl);
        }
    }
}
