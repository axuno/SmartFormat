using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json.Linq;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;

namespace SmartFormat.Performance
{
    /*
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET Core SDK=5.0.202
  [Host]        : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=.NET Core 5.0  Runtime=.NET Core 5.0

|             Method |     N |        Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |   Allocated |
|------------------- |------ |------------:|----------:|----------:|------:|--------:|----------:|------:|------:|------------:|
| DirectMemberAccess |  1000 |    261.5 us |   5.13 us |   8.71 us |  0.18 |    0.01 |   20.9961 |     - |     - |   171.88 KB |
|     SfWithLiterals |  1000 |  1,469.5 us |  27.41 us |  29.33 us |  1.00 |    0.00 |  142.5781 |     - |     - |  1179.69 KB |
|  SfCacheReflection |  1000 |  3,712.0 us |  67.06 us |  62.73 us |  2.53 |    0.08 |  214.8438 |     - |     - |  1757.81 KB |
|   SfWithDictionary |  1000 |  2,797.9 us |  40.06 us |  37.47 us |  1.90 |    0.05 |  339.8438 |     - |     - |  2804.69 KB |
|         SfWithJson |  1000 |  2,040.5 us |  35.86 us |  33.54 us |  1.39 |    0.04 |  175.7813 |     - |     - |  1445.31 KB |
|SfNoCacheReflection |  1000 | 13,091.9 us | 129.38 us | 121.02 us |  9.29 |    0.12 |  781.2500 |     - |     - |  6468.75 KB |
|                    |       |             |           |           |       |         |           |       |       |             |
| DirectMemberAccess | 10000 |  2,519.2 us |  49.85 us |  53.34 us |  0.18 |    0.00 |  207.0313 |     - |     - |  1718.75 KB |
|     SfWithLiterals | 10000 | 13,995.4 us | 182.36 us | 170.58 us |  1.00 |    0.00 | 1437.5000 |     - |     - | 11796.88 KB |
|  SfCacheReflection | 10000 | 36,312.6 us | 438.96 us | 389.12 us |  2.59 |    0.04 | 2142.8571 |     - |     - | 17578.13 KB |
|   SfWithDictionary | 10000 | 27,728.2 us | 432.64 us | 404.69 us |  1.98 |    0.03 | 3406.2500 |     - |     - | 28046.88 KB |
|         SfWithJson | 10000 | 20,671.7 us | 170.46 us | 159.45 us |  1.48 |    0.02 | 1750.0000 |     - |     - | 14453.13 KB |
|SfNoCacheReflection | 10000 |130,049.2 us |1,231.06 us|1,027.99us |  9.24 |    0.16 | 7750.0000 |     - |     - | 64687.81 KB |

* Legends *
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

    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class SourcePerformanceTests
    {
        private const string _format = "Address: {City.ZipCode} {City.Name}, {City.AreaCode}\n" +
                              "Name: {Person.FirstName} {Person.LastName}";

        private const string _formatForLiteral = "Address: {0} {1}, {2}\nName: {3} {4}";

        private readonly SmartFormatter _literalFormatter;
        private readonly SmartFormatter _reflectionFormatter;
        private readonly SmartFormatter _dictionaryFormatter;
        private readonly SmartFormatter _jsonFormatter;
        
        private readonly Address _reflectionAddress = new Address();
        private readonly Dictionary<string, object> _dictionaryAddress = new Address().ToDictionary();
        private readonly JObject _jsonAddress = new Address().ToJson();

        private FormatCache _formatCache;
        private FormatCache _formatCacheLiteral;

        public SourcePerformanceTests()
        {
            _literalFormatter = new SmartFormatter();
            _literalFormatter.AddExtensions(
                new DefaultSource(_literalFormatter)
            );
            _literalFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _reflectionFormatter = new SmartFormatter();
            _reflectionFormatter.AddExtensions(
                new ReflectionSource(_reflectionFormatter),
                new DefaultSource(_reflectionFormatter)
            );
            _reflectionFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _dictionaryFormatter = new SmartFormatter();
            _dictionaryFormatter.AddExtensions(
                new DictionarySource(_dictionaryFormatter),
                new DefaultSource(_dictionaryFormatter)
            );
            _dictionaryFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _jsonFormatter = new SmartFormatter();
            _jsonFormatter.AddExtensions(
                new SystemTextJsonSource(_jsonFormatter),
                new DefaultSource(_jsonFormatter)
            );
            _jsonFormatter.AddExtensions(
                new DefaultFormatter()
            );

            // Cache the parsing result, so we don't include parsing performance
            var format = _jsonFormatter.Parser.ParseFormat(_format);
            _formatCache = new FormatCache(format);

            var formatForLiteral = _jsonFormatter.Parser.ParseFormat(_formatForLiteral);
            _formatCacheLiteral = new FormatCache(formatForLiteral);

        }

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
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
        public void SfWithLiterals()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _literalFormatter.FormatWithCache(ref _formatCacheLiteral,"Address: {0} {1}, {2}\nName: {3} {4}", _reflectionAddress.City.ZipCode,
                    _reflectionAddress.City.Name, _reflectionAddress.City.AreaCode,
                    _reflectionAddress.Person.FirstName, _reflectionAddress.Person.LastName);
            }
        }

        [Benchmark]
        public void SfCacheReflection()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _reflectionFormatter.FormatWithCache(ref _formatCache, _format, _reflectionAddress);
            }
        }

        [Benchmark]
        public void SfWithDictionary()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _dictionaryFormatter.FormatWithCache(ref _formatCache, _format, _dictionaryAddress);
            }
        }

        [Benchmark]
        public void SfWithJson()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _jsonFormatter.FormatWithCache(ref _formatCache, _format, _jsonAddress);
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
