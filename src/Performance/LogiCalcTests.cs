using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Performance;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
// [RPlotExporter]
public class LogiCalcTests
{
    private const string CondFmtString = "{0:cond:Zero|Two}";
    private const string ChooseFmtString = "{0:choose(0|2):Zero|Two}";
    private const string LogicCalcFmtString = "{0:M:{}=0?'Zero':'Two'}";

    private readonly Format _condFormat;
    private readonly Format _chooseFormat;
    private readonly Format _logicCalcFormat;

    private Parser _parser = new(new SmartSettings());
    private static SmartFormatter _condFormatter = new(new SmartSettings());
    private static SmartFormatter _chooseFormatter = new(new SmartSettings());
    private static SmartFormatter _logicCalcFormatter = new(new SmartSettings());
/*
// * Summary *

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.22000
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.400
  [Host]   : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT
  .NET 6.0 : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0

    /*
** NCalc enabled, with Parser.ParseFormat if needed, static StringBuilder, string.Join for Parameter-Name (pName)
|    Method |   N |       Mean |     Error |    StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |---- |-----------:|----------:|----------:|--------:|------:|------:|----------:|
|      Cond |  10 |   7.273 us | 0.0950 us | 0.0889 us |  0.3204 |     - |     - |      3 KB |
|    Choose |  10 |   8.368 us | 0.0806 us | 0.0754 us |  0.5493 |     - |     - |      5 KB |
| LogicCalc |  10 |  16.688 us | 0.2579 us | 0.2286 us |  1.0986 |     - |     - |      9 KB |
| PureNCalc |  10 |   4.224 us | 0.0781 us | 0.0692 us |  0.7706 |     - |     - |      6 KB |
|      Cond | 100 |  72.114 us | 1.0332 us | 0.9665 us |  3.1738 |     - |     - |     27 KB |
|    Choose | 100 |  87.418 us | 1.2041 us | 1.1263 us |  5.4932 |     - |     - |     45 KB |
| LogicCalc | 100 | 167.624 us | 1.8362 us | 1.6277 us | 10.9863 |     - |     - |     91 KB |
| PureNCalc | 100 |  40.607 us | 0.8121 us | 1.1903 us |  7.6904 |     - |     - |     63 KB |

** NCalc enabled, with Parser.ParseFormat if needed, ZStringBuilder, string.Join für Parameter-Name (pName)
|    Method |   N |       Mean |     Error |    StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |---- |-----------:|----------:|----------:|--------:|------:|------:|----------:|
|      Cond |  10 |   7.618 us | 0.1267 us | 0.1123 us |  0.3204 |     - |     - |      3 KB |
|    Choose |  10 |   9.268 us | 0.1793 us | 0.1761 us |  0.5493 |     - |     - |      5 KB |
| LogicCalc |  10 |  19.665 us | 0.3783 us | 0.4504 us |  1.1292 |     - |     - |      9 KB |
| PureNCalc |  10 |   4.300 us | 0.0723 us | 0.0676 us |  0.7706 |     - |     - |      6 KB |
|      Cond | 100 |  79.387 us | 1.2769 us | 1.1944 us |  3.1738 |     - |     - |     27 KB |
|    Choose | 100 |  90.538 us | 1.7370 us | 2.1331 us |  5.4932 |     - |     - |     45 KB |
| LogicCalc | 100 | 192.103 us | 3.6876 us | 3.2689 us | 11.4746 |     - |     - |     94 KB |
| PureNCalc | 100 |  40.607 us | 0.8121 us | 1.1903 us |  7.6904 |     - |     - |     63 KB |

** NCalc disabled, with Parser.ParseFormat if needed
|    Method |   N |      Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |---- |----------:|----------:|----------:|-------:|------:|------:|----------:|
|      Cond |  10 |  8.440 us | 0.0904 us | 0.0846 us | 0.3204 |     - |     - |      3 KB |
|    Choose |  10 |  9.761 us | 0.0771 us | 0.0721 us | 0.5493 |     - |     - |      5 KB |
| LogicCalc |  10 |  8.268 us | 0.0705 us | 0.0660 us | 0.3815 |     - |     - |      3 KB |
| PureNCalc |  10 |  4.384 us | 0.0574 us | 0.0537 us | 0.7706 |     - |     - |      6 KB |
|      Cond | 100 | 83.670 us | 0.7156 us | 0.6694 us | 3.1738 |     - |     - |     27 KB |
|    Choose | 100 | 95.886 us | 1.1790 us | 1.0452 us | 5.4932 |     - |     - |     45 KB |
| LogicCalc | 100 | 81.866 us | 1.0033 us | 0.9385 us | 3.7842 |     - |     - |     31 KB |
| PureNCalc | 100 | 40.607 us | 0.8121 us | 1.1903 us | 7.6904 |     - |     - |     63 KB |

// * Hints *
Outliers
  LogicCalcTests.LogicCalc: .NET 6.0 -> 1 outlier  was  removed (17.47 us)
  LogicCalcTests.PureNCalc: .NET 6.0 -> 1 outlier  was  removed (4.43 us)
  LogicCalcTests.LogicCalc: .NET 6.0 -> 1 outlier  was  removed (173.20 us) => first time compilation

// * Legends *
  N         : Value of the 'N' parameter
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Gen 0     : GC Generation 0 collects per 1000 operations
  Gen 1     : GC Generation 1 collects per 1000 operations
  Gen 2     : GC Generation 2 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
*/
    public LogiCalcTests()
    {
        _condFormat = _parser.ParseFormat(CondFmtString);
        _chooseFormat = _parser.ParseFormat(ChooseFmtString);
        _logicCalcFormat = _parser.ParseFormat(LogicCalcFmtString);

        _condFormatter
            .AddExtensions(
                //new StringSource(),
                // will automatically be added to the IFormatter list, too
                //new ListFormatter(),
                //new DictionarySource(),
                //new ValueTupleSource(),
                //new ReflectionSource(),
                // for string.Format behavior
                new DefaultSource()//,
                //new KeyValuePairSource()
            )
            .AddExtensions(
                //new PluralLocalizationFormatter(),
                new ConditionalFormatter(),
                //new IsMatchFormatter(),
                //new NullFormatter(),
                //new ChooseFormatter(),
                //new SubStringFormatter(),
                // for string.Format behavior
                new DefaultFormatter()
            );

        _chooseFormatter
            .AddExtensions(
                //new StringSource(),
                // will automatically be added to the IFormatter list, too
                //new ListFormatter(),
                //new DictionarySource(),
                //new ValueTupleSource(),
                //new ReflectionSource(),
                // for string.Format behavior
                new DefaultSource()//,
                //new KeyValuePairSource()
            )
            .AddExtensions(
                //new PluralLocalizationFormatter(),
                //new ConditionalFormatter(),
                //new IsMatchFormatter(),
                //new NullFormatter(),
                new ChooseFormatter(),
                //new SubStringFormatter(),
                // for string.Format behavior
                new DefaultFormatter()
            );

        _logicCalcFormatter
            .AddExtensions(
                //new StringSource(),
                // will automatically be added to the IFormatter list, too
                //new ListFormatter(),
                //new DictionarySource(),
                //new ValueTupleSource(),
                //new ReflectionSource(),
                // for string.Format behavior
                new DefaultSource()//,
                //new KeyValuePairSource()
            )
            .AddExtensions(
                //new PluralLocalizationFormatter(),
                //new ConditionalFormatter(),
                //new IsMatchFormatter(),
                //new NullFormatter(),
                //new ChooseFormatter(),
                new LogiCalcFormatter(),
                //new SubStringFormatter(),
                // for string.Format behavior
                new DefaultFormatter()
            );
    }

    [Params(10, 100)] public int N;

    [GlobalSetup]
    public void Setup()
    {

    }

    [Benchmark]
    public void Cond()
    {
        for (var i = 0; i < N; i++)
        {
            _ = _condFormatter.Format(_condFormat, 2);
        }
    }

    [Benchmark]
    public void Choose()
    {
        for (var i = 0; i < N; i++)
        {
            _ = _chooseFormatter.Format(_chooseFormat, 2);
        }
    }

    [Benchmark]
    public void LogicCalc()
    {
        for (var i = 0; i < N; i++)
        {
            _ = _logicCalcFormatter.Format(_logicCalcFormat, 2);
        }
    }

/*
Using: DynamicExpresso with l.Invoke(parameter);
|          Method |   N |       Mean |     Error |    StdDev |     Median |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------- |---- |-----------:|----------:|----------:|-----------:|--------:|-------:|------:|----------:|
|       PureNCalc |  10 |   3.962 us | 0.0781 us | 0.1095 us |   3.899 us |  0.7706 |      - |     - |      6 KB |
| PureDynExpresso |  10 | 191.074 us | 3.7918 us | 8.8633 us | 185.416 us |  7.8125 | 2.6855 |     - |     65 KB |
|       PureNCalc | 100 |  40.729 us | 0.7110 us | 0.6651 us |  40.425 us |  7.6904 |      - |     - |     63 KB |
| PureDynExpresso | 100 | 266.201 us | 2.2993 us | 1.9200 us | 265.286 us | 14.1602 | 3.9063 |     - |    119 KB |

Using: DynamicExpresso with t.Eval<string>(parameter);
|          Method |   N |          Mean |       Error |      StdDev |        Median |    Gen 0 |    Gen 1 | Gen 2 | Allocated |
|---------------- |---- |--------------:|------------:|------------:|--------------:|---------:|---------:|------:|----------:|
|       PureNCalc |  10 |      4.022 us |   0.0753 us |   0.0668 us |      4.001 us |   0.7706 |        - |     - |      6 KB |
| PureDynExpresso |  10 |  1,594.676 us |  31.7938 us |  70.4529 us |  1,556.132 us |  48.8281 |  23.4375 |     - |    414 KB |
|       PureNCalc | 100 |     40.669 us |   0.8008 us |   1.2467 us |     40.340 us |   7.6904 |        - |     - |     63 KB |
| PureDynExpresso | 100 | 16,066.776 us | 320.0519 us | 760.6373 us | 15,878.786 us | 468.7500 | 234.3750 |     - |  3,947 KB |
 */
    [Benchmark]
    public void PureNCalc()
    {
        NCalc.Expression e; // keep references to cached expressions alive
        for (var i = 0; i < N; i++)
        {
            e = new NCalc.Expression("[0]=0?'Zero':'Two'", NCalc.EvaluateOptions.None, CultureInfo.InvariantCulture);
            e.Parameters.Add("0", 2);
            _ = e.Evaluate();
        }
    }
}
