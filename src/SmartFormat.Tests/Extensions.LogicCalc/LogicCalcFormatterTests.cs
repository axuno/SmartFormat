using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Tests.Extensions;

[TestFixture]
public class LogicCalcFormatterTests
{
    private static SmartFormatter GetFormatter()
    {
        var smart = Smart.CreateDefaultSmartFormat(new SmartSettings
        {
            Parser = new ParserSettings {ErrorAction = ParseErrorAction.ThrowError},
            Formatter = new FormatterSettings {ErrorAction = FormatErrorAction.ThrowError}
        });

        if (smart.GetFormatterExtension<LogicCalcFormatter>() is null)
        {
            smart.AddExtensions(new LogicCalcFormatter());
        }

        return smart;
    }

    [TestCase("{:M:123}", "en", "123")]
    [TestCase("{:M:123.456}", "en", "123.456")]
    [TestCase("{:M:.456}", "", "0.456")]
    [TestCase("{:M(0.0):.456}", "", "0.5")]
    // NCalc DateTime literal
    [TestCase("{:M:#2022/12/31#}", "", "12/31/2022 00:00:00")]
    [TestCase("{:M:.456}", "en", "0.456")]
    [TestCase("{:M:#2022/12/31#}", "en", "12/31/2022 12:00:00 AM")]
    [TestCase("{:M:.456}", "es", "0,456")]
    [TestCase("{:M:#2022/12/31 08:00:00#}", "es", "31/12/2022 8:00:00")]
    [TestCase("{:M:'some text'}", "es", "some text")]
    public void Evaluate_Simple_Literals(string format, string culture, string expected)
    {
        var smart = GetFormatter();
        var result = smart.Format(CultureInfo.GetCultureInfo(culture), format);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("{:M:11+33}", "44")]
    [TestCase("{:M: 11 + 33 }", "44")]
    [TestCase("{:M: Max(5, 9) }", "9")]
    [TestCase("{:M: 5 < 9 }", "True")]
    [TestCase("{:M: in (2+3, 1, 3, 5, 7) }", "True")]
    [TestCase("{:M: if (3 % 2 = 1, 'value is true', 'value is false') }", "value is true")]
    [TestCase("{:M:'some' + ' text'}", "some text")]
    public void Evaluate_Literal_Calculations(string format, string expected)
    {
        var smart = GetFormatter();
        var result = smart.Format(format);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("{:M:{0}+{1}}", "44", 2)]
    [TestCase("{:M: in ({0}, 10, 11, 12) }", "True", 1)]
    // NCalc empty literal must come before the parameter
    [TestCase("{:M: if ({1} % {0} = 0, '' + {1} + ' % ' + {0} + ' = 0', 'false') }", "33 % 11 = 0", 2)]
    [TestCase("{0:M: if ({1} % {} = 0, '' + {1} + ' % ' + {} + ' = 0', 'false') }", "33 % 11 = 0", 2)]
    public void Evaluate_Indexed_Placeholders_As_NCalc_Parameters(string format, string expected, int paraCount)
    {
        var smart = GetFormatter();
        var nc = (LogicCalcFormatter) smart.GetFormatterExtensions().First(f => f.GetType() == typeof(LogicCalcFormatter));
        var result = smart.Format(format, 11, 33);
        Assert.That(result, Is.EqualTo(expected));
        Assert.That(nc.LastNCalcParameters?.Count, Is.EqualTo(paraCount));
    }

    [TestCase("{:M:{One}+{ChildOne.Two}}", "3", 2)]
    // ChildOne.ChildTwo.Three is nested with ListFormatter. This is not an NCalc parameter, but a string!
    [TestCase("{:M: in ({ChildOne.Two}, {ChildOne.ChildTwo.Three:list:{}|,|,}) }", "True", 1)]
    [TestCase("{One:M: if ({} = 1, {}, 'not one') }", "1", 1)]
    [TestCase("{One:M: if ({} = 9, {}, 'maybe ' + {ChildOne.Two}) }", "maybe 2", 2)]
    // List index can also be used as NCalc parameter
    [TestCase("{:M:{ChildOne.ChildTwo.Three[0]}}", "8", 1)]
    [TestCase("{:M: Max({One},{ChildOne.Two}) }", "2", 2)]
    public void Evaluate_Named_Placeholders_As_NCalc_Parameters(string format, string expected, int paraCount)
    {
        var data = new {
            One = 1,
            ChildOne = new {
                Two = 2,
                ChildTwo = new {
                    Three = new[] {8,6,4,2}
                }
            }
        };

        var smart = GetFormatter();
        var nc = (LogicCalcFormatter) smart.GetFormatterExtensions().First(f => f.GetType() == typeof(LogicCalcFormatter));
        var result = smart.Format(format, data);
        Assert.That(result, Is.EqualTo(expected));
        Assert.That(nc.LastNCalcParameters?.Count, Is.EqualTo(paraCount));
    }

    [Test]
    public void Evaluate_Custom_Evaluation_Function()
    {
        var data = new {
            One = 100,
            Two = 200
        };

        var smart = GetFormatter();
        var nc = (LogicCalcFormatter) smart.GetFormatterExtensions().First(f => f.GetType() == typeof(LogicCalcFormatter));
        nc.EvaluateFunction += (name, args) =>
        {
            if (name == "MyFunction") args.Result = ((int) args.Parameters[0].Evaluate()) / 5;
        };

        const string format = "{:M:MyFunction({One})}";
        
        var result = smart.Format(format, data);
        Assert.That(result, Is.EqualTo((data.One / 5).ToString()));
    }

    [Test]
    public void Evaluate_Custom_Parameter_Function()
    {
        var smart = GetFormatter();
        var nc = (LogicCalcFormatter) smart.GetFormatterExtensions().First(f => f.GetType() == typeof(LogicCalcFormatter));
        nc.EvaluateParameter += (name, args) =>
        {
            if (name == "MyParameter") args.Result = 1234567;
        };

        const string format = "{:M:[MyParameter]}";
        
        var result = smart.Format(format);
        Assert.That(result, Is.EqualTo("1234567"));
    }

    [Test]
    public void Illegal_NCalc_Operation_Should_Throw()
    {
        var smart = GetFormatter();
        const string format = "{:M:{0}+{1}}"; // try to add integer and string
        Assert.That(code: () => smart.Format(format, 123, "abc"),
            Throws.TypeOf<FormattingException>()
                .And.InnerException.TypeOf<FormatException>()); // Thrown by NCalc
    }
}
