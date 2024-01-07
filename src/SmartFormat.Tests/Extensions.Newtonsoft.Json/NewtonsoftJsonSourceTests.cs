using System.Globalization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class NewtonsoftJsonSourceTests
{
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

    private static SmartFormatter GetFormatterWithJsonSource(SmartSettings? settings = null)
    {
        var smart = new SmartFormatter(settings ?? new SmartSettings())
            // NewtonsoftJsonSource MUST be registered before ReflectionSource (which is not required here)
            // We also need the ListFormatter to process arrays
            .AddExtensions(new ListFormatter(), new NewtonsoftJsonSource(), new DefaultSource())
            .AddExtensions(new ListFormatter(), new NullFormatter(), new DefaultFormatter());
        return smart;
    }

    [Test]
    public void Format_Null_Json()
    {
        var jObject = JObject.Parse(JsonNull);
        var result = GetFormatterWithJsonSource().Format("{Name:isnull:Value is Null|{}}", jObject);
        Assert.That(result, Is.EqualTo("Value is Null"));
    }

    [Test]
    public void Format_JsonValue()
    {
        var smart = new SmartFormatter()
            // removed DefaultSource in order to evaluate JValue
            .AddExtensions(new NewtonsoftJsonSource())
            .AddExtensions(new DefaultFormatter());
        var result = smart.Format("{0}", new JValue('v'));
        Assert.That(result, Is.EqualTo("v"));
    }

    [Test]
    public void Format_OneLevel_Json()
    {
        var jObject = JObject.Parse(JsonOneLevel);
        var result = GetFormatterWithJsonSource().Format("{Name}", jObject);
        Assert.That(result, Is.EqualTo("Doe"));
    }

    [Test]
    public void Format_TwoLevel_Json()
    {
        var jObject = JObject.Parse(JsonTwoLevel);
        var result = GetFormatterWithJsonSource().Format("{Name.First}", jObject);
        Assert.That(result, Is.EqualTo("Joe"));
    }

    [Test]
    public void Format_TwoLevel_Nullable_Json()
    {
        var jObject = JObject.Parse(JsonNull);
        var result = GetFormatterWithJsonSource().Format("{Name?.First}", jObject);
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void Format_Complex_Json()
    {
        var jObject = JObject.Parse(JsonComplex);
        var smart = GetFormatterWithJsonSource(new SmartSettings {CaseSensitivity = CaseSensitivityType.CaseSensitive});
        Assert.Multiple(() =>
        {
            Assert.That(smart.Format(CultureInfo.InvariantCulture, "{Manufacturers[0].Products[0].Price:0.00}", jObject), Is.EqualTo("50.00"));
            Assert.That(smart.Format(CultureInfo.InvariantCulture, "{Manufacturers[1].Products[0].OnStock}", jObject), Is.EqualTo("True"));
            Assert.That(smart.Format(CultureInfo.InvariantCulture, "{Manufacturers[1].Products[1].OnStock}", jObject), Is.EqualTo("False"));
        });
    }

    [Test]
    public void Format_Complex_Json_CaseInsensitive()
    {
        var jObject = JObject.Parse(JsonComplex);
        var smart = GetFormatterWithJsonSource(new SmartSettings {CaseSensitivity = CaseSensitivityType.CaseInsensitive});
        var result = smart.Format(CultureInfo.InvariantCulture, "{MaNuFaCtUrErS[0].PrOdUcTs[0].PrIcE:0.00}", jObject);
        Assert.That(result, Is.EqualTo("50.00"));
    }

    [Test]
    public void Format_List_Json()
    {
        var jObject = JObject.Parse(JsonComplex);
        var result = GetFormatterWithJsonSource().Format("{Stores:list:{}|, |, and }", jObject);
        Assert.That(result, Is.EqualTo("Lambton Quay, and Willis Street"));
    }

    [Test]
    public void Format_Exception_Json()
    {
        var smart = GetFormatterWithJsonSource(new SmartSettings {Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}});
        var jObject = JObject.Parse(JsonOneLevel);
        Assert.Throws<FormattingException>(() => smart.Format("{Dummy}", jObject));
    }
}