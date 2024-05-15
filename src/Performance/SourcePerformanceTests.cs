using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json.Linq;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Performance
{
    /*
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1165 (21H1/May2021Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=5.0.302
  [Host]   : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  .NET 5.0 : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT

Job=.NET 5.0  Runtime=.NET 5.0

|            Method |     N |        Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------ |------ |------------:|----------:|----------:|------:|--------:|----------:|------:|------:|----------:|
|       PureParsing |  1000 |    990.1 us |  19.47 us |  23.91 us |  1.00 |    0.00 |  220.7031 |     - |     - |  1,805 KB |
|    SfWithLiterals |  1000 |  1,175.3 us |  14.24 us |  13.32 us |  1.18 |    0.04 |   89.8438 |     - |     - |    734 KB |
| SfCacheReflection |  1000 |  3,081.6 us |  38.08 us |  35.62 us |  3.10 |    0.08 |  121.0938 |     - |     - |  1,016 KB |
|  SfWithDictionary |  1000 |  2,177.2 us |  16.31 us |  15.26 us |  2.19 |    0.06 |  210.9375 |     - |     - |  1,750 KB |
|        SfWithJson |  1000 |  1,813.4 us |  33.52 us |  31.35 us |  1.82 |    0.06 |   85.9375 |     - |     - |    703 KB |
|                   |       |             |           |           |       |         |           |       |       |           |
|       PureParsing | 10000 | 10,029.5 us | 129.46 us | 121.10 us |  1.00 |    0.00 | 2203.1250 |     - |     - | 18,047 KB |
|    SfWithLiterals | 10000 | 11,316.1 us | 128.92 us | 120.59 us |  1.13 |    0.02 |  890.6250 |     - |     - |  7,344 KB |
| SfCacheReflection | 10000 | 30,835.3 us | 193.57 us | 181.06 us |  3.08 |    0.05 | 1218.7500 |     - |     - | 10,156 KB |
|  SfWithDictionary | 10000 | 20,984.4 us | 266.03 us | 248.84 us |  2.09 |    0.03 | 2125.0000 |     - |     - | 17,500 KB |
|        SfWithJson | 10000 | 17,390.1 us | 174.58 us | 163.30 us |  1.73 |    0.03 |  843.7500 |     - |     - |  7,031 KB |

// * Hints *
Outliers
  SourcePerformanceTests.PureParsing: .NET 5.0      -> 3 outliers were removed (1.08 ms..1.18 ms)
  SourcePerformanceTests.SfWithDictionary: .NET 5.0 -> 2 outliers were detected (2.14 ms, 2.15 ms)
  SourcePerformanceTests.SfWithJson: .NET 5.0       -> 2 outliers were detected (1.74 ms, 1.75 ms)

// * Legends *
  N         : Value of the 'N' parameter
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Ratio     : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD   : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Gen 0     : GC Generation 0 collects per 1000 operations
  Gen 1     : GC Generation 1 collects per 1000 operations
  Gen 2     : GC Generation 2 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
*/

    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class SourcePerformanceTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divided upon lesser a.";
        private const string Format = "Address: {City.ZipCode} {City.Name}, {City.AreaCode}\n" +
                              "Name: {Person.FirstName} {Person.LastName}";

        private const string FormatForLiteral = "Address: {0} {1}, {2}\nName: {3} {4}";

        private readonly SmartFormatter _literalFormatter;
        private readonly SmartFormatter _reflectionFormatter;
        private readonly SmartFormatter _dictionaryFormatter;
        private readonly SmartFormatter _jsonFormatter;
        private readonly Parser _pureParsingParser = new Parser(new SmartSettings());

        private readonly Address _reflectionAddress = new();
        private readonly Dictionary<string, object> _dictionaryAddress = new Address().ToDictionary();
        private readonly JObject _jsonAddress = new Address().ToJson();

        private readonly Format _formatCache;
        private readonly Format _formatCacheLiteral;

        public SourcePerformanceTests()
        {
            _literalFormatter = new SmartFormatter();
            _literalFormatter.AddExtensions(
                new DefaultSource()
            );
            _literalFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _reflectionFormatter = new SmartFormatter();
            _reflectionFormatter.AddExtensions(
                new ReflectionSource(),
                new DefaultSource()
            );
            _reflectionFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _dictionaryFormatter = new SmartFormatter();
            _dictionaryFormatter.AddExtensions(
                new DictionarySource(),
                new DefaultSource()
            );
            _dictionaryFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _jsonFormatter = new SmartFormatter();
            _jsonFormatter.AddExtensions(
                new NewtonsoftJsonSource(),
                new DefaultSource()
            );
            _jsonFormatter.AddExtensions(
                new DefaultFormatter()
            );

            // Cache the parsing result, so we don't include parsing performance
            _formatCache = _jsonFormatter.Parser.ParseFormat(Format);;

            _formatCacheLiteral = _jsonFormatter.Parser.ParseFormat(FormatForLiteral);

        }

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
        }

        //[Benchmark]
        public void DirectMemberAccess()
        {
            for (var i = 0; i < N; i++)
            {
                _ = string.Format("Address: {0} {1}, {2}\nName: {3} {4}", _reflectionAddress.City.ZipCode,
                    _reflectionAddress.City.Name, _reflectionAddress.City.AreaCode,
                    _reflectionAddress.Person.FirstName, _reflectionAddress.Person.LastName);
            }
        }

        [Benchmark(Baseline = true)]
        public void PureParsing()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _pureParsingParser.ParseFormat("{City}{City}{City}{FirstName}{LastName}");  // "{City}{City}{City}{FirstName}{LastName}"
            }
        }

        [Benchmark]
        public void SfWithLiterals()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _literalFormatter.Format(_formatCacheLiteral, _reflectionAddress.City.ZipCode,
                    _reflectionAddress.City.Name, _reflectionAddress.City.AreaCode,
                    _reflectionAddress.Person.FirstName, _reflectionAddress.Person.LastName);
            }
        }

        [Benchmark]
        public void SfCacheReflection()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _reflectionFormatter.Format(_formatCache, _reflectionAddress);
            }
        }

        [Benchmark]
        public void SfWithDictionary()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _dictionaryFormatter.Format(_formatCache, _dictionaryAddress);
            }
        }

        [Benchmark]
        public void SfWithJson()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _jsonFormatter.Format(_formatCache, _jsonAddress);
            }
        }

        public class Address
        {
            public CityDetails City { get; set; } = new CityDetails();
            public PersonDetails Person { get; set; } = new PersonDetails();

            public Dictionary<string, object> ToDictionary()
            {
                var d = new Dictionary<string, object>
                {
                    { nameof(City), City.ToDictionary() },
                    { nameof(Person), Person.ToDictionary() }
                };
                return d;
            }

            public JObject ToJson()
            {
                return JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(this));
            }

            public class CityDetails
            {
                public string Name { get; set; } = "New York";
                public string ZipCode { get; set; } = "00501";
                public string AreaCode { get; set; } = "631";

                public Dictionary<string, string> ToDictionary()
                {
                    return new()
                    {
                        {nameof(Name), Name},
                        {nameof(ZipCode), ZipCode},
                        {nameof(AreaCode), AreaCode}
                    };
                }
            }

            public class PersonDetails
            {
                public string FirstName { get; set; } = "John";
                public string LastName { get; set; } = "Doe";
                public Dictionary<string, string> ToDictionary()
                {
                    return new()
                    {
                        {nameof(FirstName), FirstName},
                        {nameof(LastName), LastName}
                    };
                }
            }
        }
    }
}
