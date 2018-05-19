using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class JsonSourceTest
    {
        private const string JsonOneLevel = @"{'Name': 'Doe'}";
        private const string JsonTwoLevel = @"{'Name': {'First': 'Joe'}}";

        private const string JsonComplex = @"{
  'Stores': [
    'Lambton Quay',
    'Willis Street'
  ],
  'Manufacturers': [
    {
      'Name': 'Acme Co',
      'Products': [
        {
          'Name': 'Anvil',
          'Price': 50
        }
      ]
    },
    {
      'Name': 'Contoso',
      'Products': [
        {
          'Name': 'Elbow Grease',
          'Price': 99.95
        },
        {
          'Name': 'Headlight Fluid',
          'Price': 4
        }
      ]
    }
  ]
}";

        [Test]
        public void Format_OneLevel_Json()
        {
            Smart.Default.SourceExtensions.Add(new JsonSource(Smart.Default));
            var jObject = JObject.Parse(JsonOneLevel);
            var result = Smart.Format("{Name}", jObject);
            Assert.AreEqual("Doe", result);
        }

        [Test]
        public void Format_TwoLevel_Json()
        {
            var jObject = JObject.Parse(JsonTwoLevel);
            var result = Smart.Format("{Name.First}", jObject);
            Assert.AreEqual("Joe", result);
        }

        [Test]
        public void Format_Complex_Json()
        {
            var jObject = JObject.Parse(JsonComplex);
            var result = Smart.Format(CultureInfo.InvariantCulture, "{Manufacturers[0].Products[0].Price:0.00}", jObject);
            Assert.AreEqual("50.00", result);
        }

        [Test]
        public void Format_Complex_Json_CaseInsensitive()
        {
            var jObject = JObject.Parse(JsonComplex);
            var savedSetting = Smart.Default.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            var result = Smart.Format(CultureInfo.InvariantCulture, "{MaNuFaCtUrErS[0].PrOdUcTs[0].PrIcE:0.00}", jObject);
            Assert.AreEqual("50.00", result);
            Smart.Default.Settings.CaseSensitivity = savedSetting;
        }

        [Test]
        public void Format_List_Json()
        {
            var jObject = JObject.Parse(JsonComplex);
            var result = Smart.Format("{Stores:list:{}|, |, and }", jObject);
            Assert.AreEqual("Lambton Quay, and Willis Street", result);
        }

        [Test]
        public void Format_Exception_Json()
        {
            var savedSetting = Smart.Default.Settings.FormatErrorAction;
            Smart.Default.Settings.FormatErrorAction = ErrorAction.ThrowError;
            var jObject = JObject.Parse(JsonOneLevel);
            Assert.Throws<FormattingException>(() => Smart.Format("{Dummy}", jObject));
            Smart.Default.Settings.FormatErrorAction = savedSetting;
        }

    }
}
