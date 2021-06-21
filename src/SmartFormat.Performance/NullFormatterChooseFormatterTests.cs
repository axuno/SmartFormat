using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Performance
{
    /*
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET Core SDK=5.0.203
  [Host]        : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT

Job=.NET Core 5.0  Runtime=.NET Core 5.0

|           Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
| ChooseFormatTest | 854.0 ns | 7.29 ns | 6.82 ns | 0.1411 |     - |     - |    1184 B |
|   NullFormatTest | 553.5 ns | 3.02 ns | 2.67 ns | 0.1030 |     - |     - |     864 B |
    */

    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    public class NullFormatterChooseFormatterTests
    {
        private SmartFormatter _smartNullFormatter, _smartChooseFormatter;
        private FormatCache _nullFormatCache, _chooseFormatCache;

        public NullFormatterChooseFormatterTests()
        {
            Setup();
        }

        [GlobalSetup]
        public void Setup()
        {
            _smartNullFormatter = new SmartFormatter();
            _smartNullFormatter.AddExtensions(new ISource[] { new DefaultSource(_smartNullFormatter) });
            _smartNullFormatter.AddExtensions(new IFormatter[] { new NullFormatter(), new DefaultFormatter()});

            _smartChooseFormatter = new SmartFormatter();
            _smartChooseFormatter.AddExtensions(new ISource[] { new DefaultSource(_smartChooseFormatter) });
            _smartChooseFormatter.AddExtensions(new IFormatter[] { new ChooseFormatter(), new DefaultFormatter()});

            _nullFormatCache = new FormatCache(_smartNullFormatter.Parser.ParseFormat("{0:isnull:nothing}"));
            _chooseFormatCache = new FormatCache(_smartChooseFormatter.Parser.ParseFormat("{0:choose(null):nothing|}"));
        }

        [Benchmark]
        public void ChooseFormatTest()
        {
            var result = _smartChooseFormatter.FormatWithCache(ref _chooseFormatCache, "", new List<object> {null});
        }

        [Benchmark]
        public void NullFormatTest()
        {
            var result = _smartNullFormatter.FormatWithCache(ref _nullFormatCache, "", new List<object> {null});
        }
    }
}