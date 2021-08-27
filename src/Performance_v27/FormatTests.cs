using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
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

StringOutput
|                   Method |     N |          Mean |       Error |      StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Gen 2 |  Allocated |
|------------------------- |------ |--------------:|------------:|------------:|------:|--------:|-----------:|----------:|------:|-----------:|
|              Placeholder |   100 |     28.127 us |   0.5576 us |   0.5216 us |  3.00 |    0.09 |     4.4861 |         - |     - |      37 KB |
|          Placeholder0005 |   100 |    105.737 us |   2.0459 us |   2.6602 us | 11.30 |    0.34 |     9.6436 |         - |     - |      80 KB |
|          Literal0010Char |   100 |      9.371 us |   0.1497 us |   0.1400 us |  1.00 |    0.00 |     3.3417 |         - |     - |      27 KB |
|          Literal3000Char |   100 |     56.754 us |   0.3694 us |   0.3085 us |  6.08 |    0.08 |    74.4629 |    1.6479 |     - |     609 KB |
|   Literal6000Placeholder |   100 |    133.468 us |   1.5960 us |   1.4929 us | 14.24 |    0.20 |   147.7051 |    6.3477 |     - |   1,209 KB |
| Literal18kUniPlaceholder |   100 |    110.975 us |   2.1944 us |   2.2535 us | 11.84 |    0.33 |   219.2383 |   13.6719 |     - |   1,797 KB |
|                          |       |               |             |             |       |         |            |           |       |            |
|              Placeholder | 10000 |  2,575.411 us |  49.2822 us |  62.3263 us |  2.87 |    0.16 |   449.2188 |         - |     - |   3,672 KB |
|          Placeholder0005 | 10000 | 10,358.460 us | 196.9912 us | 241.9229 us | 11.54 |    0.47 |   968.7500 |         - |     - |   7,969 KB |
|          Literal0010Char | 10000 |    900.956 us |  17.6099 us |  27.9311 us |  1.00 |    0.00 |   333.9844 |         - |     - |   2,734 KB |
|          Literal3000Char | 10000 |  4,255.801 us |  79.6409 us |  74.4961 us |  4.80 |    0.20 |  7445.3125 |  164.0625 |     - |  60,938 KB |
|   Literal6000Placeholder | 10000 | 13,406.469 us |  96.2487 us |  90.0311 us | 15.13 |    0.61 | 14765.6250 |  640.6250 |     - | 120,859 KB |
| Literal18kUniPlaceholder | 10000 | 11,653.527 us | 124.2303 us | 110.1269 us | 13.18 |    0.51 | 21921.8750 | 1359.3750 |     - | 179,688 KB |

NullOutput
|                   Method |     N |          Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |------ |--------------:|-----------:|-----------:|------:|--------:|---------:|------:|------:|----------:|
|              Placeholder |   100 |     25.129 us |  0.2449 us |  0.2290 us |  4.41 |    0.22 |   3.3264 |     - |     - |     27 KB |
|          Placeholder0005 |   100 |     95.053 us |  1.8962 us |  1.9473 us | 16.53 |    1.00 |   7.4463 |     - |     - |     62 KB |
|          Literal0010Char |   100 |      5.918 us |  0.1180 us |  0.2329 us |  1.00 |    0.00 |   2.0065 |     - |     - |     16 KB |
|          Literal3000Char |   100 |      6.079 us |  0.1213 us |  0.1191 us |  1.06 |    0.05 |   2.0065 |     - |     - |     16 KB |
|   Literal6000Placeholder |   100 |     27.199 us |  0.5289 us |  0.6091 us |  4.68 |    0.23 |   3.3264 |     - |     - |     27 KB |
| Literal18kUniPlaceholder |   100 |     27.922 us |  0.4137 us |  0.3870 us |  4.90 |    0.23 |   3.3264 |     - |     - |     27 KB |
|                          |       |               |            |            |       |         |          |       |       |           |
|              Placeholder | 10000 |  2,306.803 us | 22.6403 us | 21.1777 us |  3.93 |    0.08 | 332.0313 |     - |     - |  2,734 KB |
|          Placeholder0005 | 10000 | 10,003.514 us | 87.6414 us | 81.9799 us | 17.03 |    0.40 | 750.0000 |     - |     - |  6,172 KB |
|          Literal0010Char | 10000 |    580.249 us | 10.7885 us | 16.1477 us |  1.00 |    0.00 | 200.1953 |     - |     - |  1,641 KB |
|          Literal3000Char | 10000 |    549.604 us | 10.8901 us | 23.4421 us |  0.96 |    0.04 | 200.1953 |     - |     - |  1,641 KB |
|   Literal6000Placeholder | 10000 |  2,752.734 us | 51.4145 us | 52.7989 us |  4.70 |    0.13 | 332.0313 |     - |     - |  2,734 KB |
| Literal18kUniPlaceholder | 10000 |  2,627.360 us | 49.5915 us | 46.3879 us |  4.47 |    0.13 | 332.0313 |     - |     - |  2,734 KB |
*/

    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class FormatTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divide";

        private Parser _parser = new SmartFormatter {Settings = {ConvertCharacterStringLiterals = true}}.Parser;
        private FormatCache _placeholderFormat;
        private FormatCache _placeholder0005Format;
        private FormatCache _literal0010CharFormat;
        private FormatCache _literal3000CharFormat;
        private FormatCache _literal6000PlaceholderFormat;
        private FormatCache _literal18kEscPlaceholderFormat;
        private SmartFormatter _literalFormatter;

        public FormatTests()
        {
            _literalFormatter = new SmartFormatter();
            _literalFormatter.AddExtensions(
                new DefaultSource(_literalFormatter)
            );
            _literalFormatter.AddExtensions(
                new DefaultFormatter()
            );

            _placeholderFormat = new FormatCache(_parser.ParseFormat("{0}", _literalFormatter.GetNotEmptyFormatterExtensionNames()));
            _placeholder0005Format = new FormatCache(_parser.ParseFormat("{0}{1}{2}{3}{4}", _literalFormatter.GetNotEmptyFormatterExtensionNames()));
            _literal0010CharFormat = new FormatCache(_parser.ParseFormat("1234567890", _literalFormatter.GetNotEmptyFormatterExtensionNames()));
            _literal3000CharFormat = new FormatCache(_parser.ParseFormat(LoremIpsum, _literalFormatter.GetNotEmptyFormatterExtensionNames()));
            _literal6000PlaceholderFormat = new FormatCache(_parser.ParseFormat(LoremIpsum + "{0}" + LoremIpsum, _literalFormatter.GetNotEmptyFormatterExtensionNames()));
            _literal18kEscPlaceholderFormat = new FormatCache(_parser.ParseFormat(LoremIpsum + @"\n" + LoremIpsum + "{0}" + LoremIpsum, _literalFormatter.GetNotEmptyFormatterExtensionNames()));

            _parser.UseAlternativeEscapeChar();
            _parser.AddAlphanumericSelectors();
            _parser.AddAdditionalSelectorChars("_-");
            _parser.AddOperators(".?,[]");
        }

        public IOutput GetOutput(int capacity)
        {
            // Note: a good estimation of the expected output length is essential for performance and GC pressure
            //return new NullOutput();
            return new StringOutput(capacity);
        }

        [Params(100, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public void Placeholder()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatWithCacheInto(ref _placeholderFormat, GetOutput(_placeholderFormat.Format.baseString.Length), string.Empty, "ph1");
            }
        }

        [Benchmark]
        public void Placeholder0005()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatWithCacheInto(ref _placeholder0005Format, GetOutput(_placeholder0005Format.Format.baseString.Length), string.Empty, "ph1", "ph2", "ph3", "ph4", "ph5");
            }
        }

        [Benchmark(Baseline = true)]
        public void Literal0010Char()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatWithCacheInto(ref _literal0010CharFormat, GetOutput(_literal0010CharFormat.Format.baseString.Length), string.Empty);
            }
        }

        [Benchmark]
        public void Literal3000Char()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatWithCacheInto(ref _literal3000CharFormat, GetOutput(_literal3000CharFormat.Format.baseString.Length), string.Empty);
            }
        }

        [Benchmark]
        public void Literal6000Placeholder()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatWithCacheInto(ref _literal6000PlaceholderFormat, GetOutput(_literal6000PlaceholderFormat.Format.baseString.Length), string.Empty, "ph1");
            }
        }

        [Benchmark]
        public void Literal18kEscPlaceholder()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatWithCacheInto(ref _literal18kEscPlaceholderFormat, GetOutput(_literal18kEscPlaceholderFormat.Format.baseString.Length), string.Empty, "ph1");
            }
        }
    }
}
