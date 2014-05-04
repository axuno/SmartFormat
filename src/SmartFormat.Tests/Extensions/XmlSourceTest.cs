using System.Xml.Linq;
using NUnit.Framework;
using SmartFormat.Extensions;

namespace SmartFormat.Tests
{
    [TestFixture]
    public class XmlSourceTest
    {

        [Test]
        public void Format_ValidXml_Formatted()
        {
            // arrange
            const string xmlStr = "<root>" +
                                  "<FirstName>Joe</FirstName><LastName>Doe</LastName>" +
                                  "</root>";
            var xmlEl = XElement.Parse(xmlStr);
            Smart.Default.AddExtensions(new XmlSource(Smart.Default));
            // act
            var res = Smart.Format("Mr. {FirstName} {LastName}", xmlEl);
            // assert
            Assert.AreEqual("Mr. Joe Doe", res);
        }
    }
}
