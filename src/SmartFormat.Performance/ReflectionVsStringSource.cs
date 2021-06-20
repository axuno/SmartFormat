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

|             Method |     N |        Mean |     Error |    StdDev |     Gen 0 | Gen 1 | Gen 2 |   Allocated |
|------------------- |------ |------------:|----------:|----------:|----------:|------:|------:|------------:|
| DirectMemberAccess |  1000 |    261.5 us |   5.13 us |   8.71 us |   20.9961 |     - |     - |   171.88 KB |
|     SfStringSource |  1000 |  2,727.5 us |  10.42 us |   9.75 us |    207.03 |     - |     - |  1695.31 KB |
|  SfCacheReflection |  1000 |  3,712.0 us |  67.06 us |  62.73 us |  214.8438 |     - |     - |  1757.81 KB |
|SfNoCacheReflection |  1000 | 13,091.9 us | 129.38 us | 121.02 us |  781.2500 |     - |     - |  6468.75 KB |
|                    |       |             |           |           |           |       |       |             |
| DirectMemberAccess | 10000 |  2,519.2 us |  49.85 us |  53.34 us |  207.0313 |     - |     - |  1718.75 KB |
|     SfStringSource | 10000 | 27,612.5 us |  68.50 us |  64.08 us | 2062.5000 |     - |     - | 16953.13 KB |
|  SfCacheReflection | 10000 | 36,312.6 us | 438.96 us |  389.12us | 2142.8571 |     - |     - | 17578.13 KB |
|SfNoCacheReflection | 10000 |130,049.2 us |1,231.06us |1,027.99us | 7750.0000 |     - |     - | 64687.81 KB |

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
    public class ReflectionVsStringSource
    {
        private const string _formatString = "Address: {0.ToUpper} {1.ToLower}, {2.Trim}";

        private readonly SmartFormatter _reflectionSourceFormatter;
        private readonly SmartFormatter _stringSourceFormatter;
        
        private readonly Address _address = new Address();

        private FormatCache _formatCacheLiteral;

        public ReflectionVsStringSource()
        {
            _reflectionSourceFormatter = new SmartFormatter();
            _reflectionSourceFormatter.AddExtensions(
                new ReflectionSource(_reflectionSourceFormatter),
                new DefaultSource(_reflectionSourceFormatter)
            );
            _reflectionSourceFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _stringSourceFormatter = new SmartFormatter();
            _stringSourceFormatter.AddExtensions(
                new StringSource(_stringSourceFormatter),
                new DefaultSource(_stringSourceFormatter)
            );
            _stringSourceFormatter.AddExtensions(
                new DefaultFormatter()
            );

            var parsedFormat = _stringSourceFormatter.Parser.ParseFormat(_formatString);
            _formatCache = new FormatCache(parsedFormat);

        }

        [Params(1000, 10000)]
        public int N;

        private readonly FormatCache _formatCache;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public void DirectMemberAccess()
        {
            for (var i = 0; i < N; i++)
            {
                _ = string.Format("Address: {0} {1}, {2}", _address.City.ZipCode.ToUpper(),
                    _address.City.Name.ToLower(), _address.City.AreaCode.Trim());
            }
        }

        [Benchmark]
        public void SfCacheReflectionSource()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _reflectionSourceFormatter.FormatWithCache(ref _formatCacheLiteral,"Address: {0} {1}, {2}", _address.City.ZipCode,
                    _address.City.Name, _address.City.AreaCode);
            }
        }

        [Benchmark]
        public void SfWithStringSource()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _stringSourceFormatter.FormatWithCache(ref _formatCacheLiteral,"Address: {0} {1}, {2}", _address.City.ZipCode,
                    _address.City.Name, _address.City.AreaCode);
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
