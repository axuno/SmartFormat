using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json.Linq;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
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
| SfWithStringSource |  1000 |  1,792.1 us |   9.97 us |   9.32 us |  167.9688 |     - |     - |  1382.81 KB |
|  SfCacheReflection |  1000 |  2,026.1 us |  20.62 us |  19.29 us |  179.6875 |     - |     - |  1476.56 KB |
|SfNoCacheReflection |  1000 | 13,091.9 us | 129.38 us | 121.02 us |  781.2500 |     - |     - |  6468.75 KB |
|                    |       |             |           |           |           |       |       |             |
| DirectMemberAccess | 10000 |  2,519.2 us |  49.85 us |  53.34 us |  207.0313 |     - |     - |  1718.75 KB |
| SfWithStringSource | 10000 | 17,886.7 us | 151.64 us | 141.84 us | 1687.5000 |     - |     - | 13828.13 KB |
|  SfCacheReflection | 10000 | 20,918.6 us | 166.88 us | 156.10 us | 1781.2500 |     - |     - | 14765.63 KB |
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

    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class ReflectionVsStringSourceTests
    {
        private const string _formatString = "Address: {0.ToUpper} {1.ToLower}, {2.Trim}";

        private readonly SmartFormatter _reflectionSourceFormatter;
        private readonly SmartFormatter _stringSourceFormatter;

        private readonly Address _address = new();

        private Format _formatCacheLiteral;

        public ReflectionVsStringSourceTests()
        {
            _reflectionSourceFormatter = new SmartFormatter();
            _reflectionSourceFormatter.AddExtensions(
                new ReflectionSource(),
                new DefaultSource()
            );
            _reflectionSourceFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _stringSourceFormatter = new SmartFormatter();
            _stringSourceFormatter.AddExtensions(
                new StringSource(),
                new DefaultSource()
            );
            _stringSourceFormatter.AddExtensions(
                new DefaultFormatter()
            );

            var parsedFormat = _stringSourceFormatter.Parser.ParseFormat(_formatString);
            _formatCache = parsedFormat;

        }

        [Params(1000, 10000)]
        public int N;

        private readonly Format _formatCache;

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
                _ = _reflectionSourceFormatter.Format(_formatCacheLiteral,"Address: {0} {1}, {2}", _address.City.ZipCode,
                    _address.City.Name, _address.City.AreaCode);
            }
        }

        [Benchmark]
        public void SfWithStringSource()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _stringSourceFormatter.Format(_formatCacheLiteral,"Address: {0} {1}, {2}", _address.City.ZipCode,
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
