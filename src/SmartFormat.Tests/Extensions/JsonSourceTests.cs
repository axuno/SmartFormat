using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class JsonSourceTests
    {
        private SmartFormatter _formatter = Smart.CreateDefaultSmartFormat();

        public JsonSourceTests()
        {
            _formatter.SourceExtensions.Add(new JsonSource(Smart.Default));
        }

        private const string JsonOneLevel = @"{'Name': 'Doe'}";
        private const string JsonTwoLevel = @"{'Name': {'First': 'Joe'}}";
        private const string JsonNull = @"{'Name': null}";
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
          'Price': 50,
          'OnStock': true
        }
      ]
    },
    {
      'Name': 'Contoso',
      'Products': [
        {
          'Name': 'Elbow Grease',
          'Price': 99.95,
          'OnStock': true
        },
        {
          'Name': 'Headlight Fluid',
          'Price': 4,
          'OnStock': false
        }
      ]
    }
  ]
}";

        #region *** NewtonSoftJson ***

        [Test]
        public void NS_Format_Null_Json()
        {
            var jObject = JObject.Parse(JsonNull);
            var result = _formatter.Format("{Name}", jObject);
            Assert.AreEqual("", result);
        }

        [Test]
        public void NS_Format_OneLevel_Json()
        {
            var jObject = JObject.Parse(JsonOneLevel);
            var result = _formatter.Format("{Name}", jObject);
            Assert.AreEqual("Doe", result);
        }

        [Test]
        public void NS_Format_TwoLevel_Json()
        {
            var jObject = JObject.Parse(JsonTwoLevel);
            var result = _formatter.Format("{Name.First}", jObject);
            Assert.AreEqual("Joe", result);
        }

        [Test]
        public void NS_Format_Complex_Json()
        {
            var jObject = JObject.Parse(JsonComplex);
            var savedSetting = Smart.Default.Settings.CaseSensitivity;
            Smart.Default.Settings.CaseSensitivity = CaseSensitivityType.CaseSensitive;
            Assert.Multiple(() =>
            {
                Assert.AreEqual("50.00", _formatter.Format(CultureInfo.InvariantCulture, "{Manufacturers[0].Products[0].Price:0.00}", jObject));
                Assert.AreEqual("True", _formatter.Format(CultureInfo.InvariantCulture, "{Manufacturers[1].Products[0].OnStock}", jObject));
                Assert.AreEqual("False", _formatter.Format(CultureInfo.InvariantCulture, "{Manufacturers[1].Products[1].OnStock}", jObject));
            });

            Smart.Default.Settings.CaseSensitivity = savedSetting;
        }

        [Test]
        public void NS_Format_Complex_Json_CaseInsensitive()
        {
            var jObject = JObject.Parse(JsonComplex);
            var savedSetting = _formatter.Settings.CaseSensitivity;
            _formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            var result = _formatter.Format(CultureInfo.InvariantCulture, "{MaNuFaCtUrErS[0].PrOdUcTs[0].PrIcE:0.00}", jObject);
            Assert.AreEqual("50.00", result);
            _formatter.Settings.CaseSensitivity = savedSetting;
        }

        [Test]
        public void NS_Format_List_Json()
        {
            var jObject = JObject.Parse(JsonComplex);
            var result = _formatter.Format("{Stores:list:{}|, |, and }", jObject);
            Assert.AreEqual("Lambton Quay, and Willis Street", result);
        }

        [Test]
        public void NS_Format_Exception_Json()
        {
            var savedSetting = Smart.Default.Settings.FormatErrorAction;
            Smart.Default.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            var jObject = JObject.Parse(JsonOneLevel);
            Assert.Throws<FormattingException>(() => _formatter.Format("{Dummy}", jObject));
            Smart.Default.Settings.FormatErrorAction = savedSetting;
        }

        #endregion

        #region *** System.Text.Json ***

        [Test]
        public void ST_Format_Null_Json()
        {
            var jObject = JsonDocument.Parse(JsonNull.Replace("'", "\"")).RootElement;
            var result = _formatter.Format("{Name}", jObject);
            Assert.AreEqual("", result);
        }

        [Test]
        public void ST_Format_OneLevel_Json()
        {
            var jObject = JsonDocument.Parse(JsonOneLevel.Replace("'", "\"")).RootElement;
            var result = _formatter.Format("{Name}", jObject);
            Assert.AreEqual("Doe", result);
        }

        [Test]
        public void ST_Format_TwoLevel_Json()
        {
            var jObject = JsonDocument.Parse(JsonTwoLevel.Replace("'", "\"")).RootElement;
            var result = _formatter.Format("{Name.First}", jObject);
            Assert.AreEqual("Joe", result);
        }

        [Test]
        public void ST_Format_Complex_Json()
        {
            var jObject = JsonDocument.Parse(JsonComplex.Replace("'", "\"")).RootElement;
            var savedSetting = Smart.Default.Settings.CaseSensitivity;
            Smart.Default.Settings.CaseSensitivity = CaseSensitivityType.CaseSensitive;
            Assert.Multiple(() =>
            {
                Assert.AreEqual("50.00", _formatter.Format(CultureInfo.InvariantCulture, "{Manufacturers[0].Products[0].Price:0.00}", jObject));
                Assert.AreEqual("True", _formatter.Format(CultureInfo.InvariantCulture, "{Manufacturers[1].Products[0].OnStock}", jObject));
                Assert.AreEqual("False", _formatter.Format(CultureInfo.InvariantCulture, "{Manufacturers[1].Products[1].OnStock}", jObject));
            });
            Smart.Default.Settings.CaseSensitivity = savedSetting;
        }

        [Test]
        public void ST_Format_Complex_Json_CaseInsensitive()
        {
            var jObject = JsonDocument.Parse(JsonComplex.Replace("'", "\"")).RootElement;
            var savedSetting = Smart.Default.Settings.CaseSensitivity;
            _formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            var result = _formatter.Format(CultureInfo.InvariantCulture, "{MaNuFaCtUrErS[0].PrOdUcTs[0].PrIcE:0.00}", jObject);
            Assert.AreEqual("50.00", result);
            Smart.Default.Settings.CaseSensitivity = savedSetting;
        }

        [Test]
        public void ST_Format_List_Json()
        {
            var jObject = JsonDocument.Parse(JsonComplex.Replace("'", "\"")).RootElement;
            var result = _formatter.Format("{Stores:list:{}|, |, and }", jObject);
            Assert.AreEqual("Lambton Quay, and Willis Street", result);
        }

        [Test]
        public void ST_Format_Exception_Json()
        {
            var savedSetting = Smart.Default.Settings.FormatErrorAction;
            Smart.Default.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            var jObject = JsonDocument.Parse(JsonOneLevel.Replace("'", "\"")).RootElement;
            Assert.Throws<FormattingException>(() => _formatter.Format("{Dummy}", jObject));
            Smart.Default.Settings.FormatErrorAction = savedSetting;
        }

        #endregion
    }
}
