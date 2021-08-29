using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Performance
{
/*
ZStringOutput
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1165 (21H1/May2021Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=5.0.302
  [Host]   : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  .NET 5.0 : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT

Job=.NET 5.0  Runtime=.NET 5.0

ZStringOutput
|                   Method |     N |         Mean |        Error |       StdDev |       Median | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |  Allocated |
|------------------------- |------ |-------------:|-------------:|-------------:|-------------:|------:|--------:|-----------:|------:|------:|-----------:|
|              Placeholder |   100 |     87.37 us |     0.641 us |     0.599 us |     87.40 us |  6.72 |    0.08 |    75.1953 |     - |     - |     615 KB |
|          Placeholder0005 |   100 |    398.50 us |    10.540 us |    30.409 us |    383.85 us | 31.09 |    2.89 |   364.7461 |     - |     - |   2,987 KB |
|          Literal0010Char |   100 |     13.01 us |     0.163 us |     0.145 us |     13.02 us |  1.00 |    0.00 |     2.6703 |     - |     - |      22 KB |
|          Literal3000Char |   100 |     45.53 us |     0.255 us |     0.284 us |     45.55 us |  3.51 |    0.04 |    73.8525 |     - |     - |     604 KB |
|   Literal6000Placeholder |   100 |    200.76 us |     3.971 us |     9.360 us |    203.50 us | 15.01 |    1.33 |   217.7734 |     - |     - |   1,784 KB |
| Literal18kUniPlaceholder |   100 |    265.27 us |     5.209 us |     7.302 us |    267.21 us | 20.50 |    0.30 |   289.5508 |     - |     - |   2,371 KB |
|                          |       |              |              |              |              |       |         |            |       |       |            |
|              Placeholder | 10000 |  7,853.00 us |   240.865 us |   710.197 us |  8,276.16 us |  6.07 |    0.61 |  7515.6250 |     - |     - |  61,484 KB |
|          Placeholder0005 | 10000 | 38,075.83 us |   619.051 us |   579.060 us | 38,046.99 us | 30.23 |    0.90 | 36461.5385 |     - |     - | 298,672 KB |
|          Literal0010Char | 10000 |  1,264.70 us |    24.446 us |    27.172 us |  1,267.18 us |  1.00 |    0.00 |   267.5781 |     - |     - |   2,188 KB |
|          Literal3000Char | 10000 |  6,250.49 us |    94.992 us |    74.163 us |  6,244.50 us |  4.94 |    0.12 |  7390.6250 |     - |     - |  60,391 KB |
|   Literal6000Placeholder | 10000 | 20,318.94 us |   312.232 us |   260.728 us | 20,316.30 us | 16.10 |    0.52 | 21781.2500 |     - |     - | 178,359 KB |
| Literal18kUniPlaceholder | 10000 | 22,573.03 us | 1,014.972 us | 2,992.667 us | 21,324.42 us | 21.50 |    0.80 | 28968.7500 |     - |     - | 237,109 KB |

StringOutput
|                   Method |     N |         Mean |      Error |     StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 | Gen 2 |  Allocated |
|------------------------- |------ |-------------:|-----------:|-----------:|------:|--------:|-----------:|----------:|------:|-----------:|
|              Placeholder |   100 |     93.01 us |   0.432 us |   0.404 us |  9.08 |    0.12 |   148.3154 |    3.4180 |     - |   1,213 KB |
|          Placeholder0005 |   100 |    449.04 us |   2.411 us |   2.256 us | 43.83 |    0.66 |   844.7266 |   93.7500 |     - |   6,922 KB |
|          Literal0010Char |   100 |     10.25 us |   0.155 us |   0.145 us |  1.00 |    0.00 |     3.9215 |         - |     - |      32 KB |
|          Literal3000Char |   100 |     63.78 us |   0.197 us |   0.154 us |  6.22 |    0.09 |   146.1182 |    3.1738 |     - |   1,196 KB |
|   Literal6000Placeholder |   100 |    239.58 us |   0.681 us |   0.637 us | 23.38 |    0.32 |   506.5918 |   41.9922 |     - |   4,145 KB |
| Literal18kUniPlaceholder |   100 |    295.77 us |   1.249 us |   1.168 us | 28.87 |    0.41 |   696.7773 |   77.1484 |     - |   5,709 KB |
|                          |       |              |            |            |       |         |            |           |       |            |
|              Placeholder | 10000 |  9,143.37 us |  45.297 us |  42.371 us |  8.94 |    0.10 | 14828.1250 |  343.7500 |     - | 121,328 KB |
|          Placeholder0005 | 10000 | 44,786.43 us | 216.074 us | 191.544 us | 43.82 |    0.44 | 84500.0000 | 9333.3333 |     - | 692,188 KB |
|          Literal0010Char | 10000 |  1,022.21 us |   9.913 us |   8.788 us |  1.00 |    0.00 |   390.6250 |         - |     - |   3,203 KB |
|          Literal3000Char | 10000 |  6,384.02 us |  47.231 us |  44.179 us |  6.25 |    0.06 | 14617.1875 |  320.3125 |     - | 119,609 KB |
|   Literal6000Placeholder | 10000 | 23,989.90 us | 128.930 us | 120.601 us | 23.46 |    0.20 | 50656.2500 | 4218.7500 |     - | 414,531 KB |
| Literal18kUniPlaceholder | 10000 | 29,419.74 us | 576.961 us | 539.689 us | 28.75 |    0.52 | 69656.2500 | 7718.7500 |     - | 570,938 KB |

NullOutput
|                   Method |     N |         Mean |       Error |      StdDev | Ratio | RatioSD |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |------ |-------------:|------------:|------------:|------:|--------:|---------:|------:|------:|----------:|
|              Placeholder |   100 |    23.903 us |   0.4604 us |   0.4728 us |  4.31 |    0.10 |   3.3264 |     - |     - |     27 KB |
|          Placeholder0005 |   100 |    85.904 us |   1.4842 us |   1.3883 us | 15.45 |    0.27 |   7.4463 |     - |     - |     62 KB |
|          Literal0010Char |   100 |     5.555 us |   0.0821 us |   0.0727 us |  1.00 |    0.00 |   2.0065 |     - |     - |     16 KB |
|          Literal3000Char |   100 |     5.707 us |   0.0422 us |   0.0395 us |  1.03 |    0.02 |   2.0065 |     - |     - |     16 KB |
|   Literal6000Placeholder |   100 |    26.772 us |   0.3257 us |   0.3047 us |  4.82 |    0.06 |   3.3264 |     - |     - |     27 KB |
| Literal18kUniPlaceholder |   100 |    32.661 us |   0.3406 us |   0.2844 us |  5.88 |    0.08 |   3.7231 |     - |     - |     30 KB |
|                          |       |              |             |             |       |         |          |       |       |           |
|              Placeholder | 10000 | 2,287.139 us |  34.5446 us |  30.6229 us |  4.23 |    0.07 | 332.0313 |     - |     - |  2,734 KB |
|          Placeholder0005 | 10000 | 8,910.154 us | 121.4094 us | 113.5664 us | 16.46 |    0.24 | 750.0000 |     - |     - |  6,172 KB |
|          Literal0010Char | 10000 |   541.432 us |   3.5534 us |   3.3238 us |  1.00 |    0.00 | 200.1953 |     - |     - |  1,641 KB |
|          Literal3000Char | 10000 |   513.758 us |   3.4679 us |   3.0742 us |  0.95 |    0.01 | 200.1953 |     - |     - |  1,641 KB |
|   Literal6000Placeholder | 10000 | 2,688.219 us |  52.3035 us |  55.9641 us |  4.95 |    0.10 | 332.0313 |     - |     - |  2,734 KB |
| Literal18kUniPlaceholder | 10000 | 3,366.668 us |  65.8929 us |  80.9224 us |  6.19 |    0.15 | 371.0938 |     - |     - |  3,047 KB |
*/

    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class FormatTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divide";

        private Parser _parser = new (new SmartSettings());
        private Format _placeholderFormat;
        private Format _placeholder0005Format;
        private Format _literal0010CharFormat;
        private Format _literal3000CharFormat;
        private Format _literal6000PlaceholderFormat;
        private Format _literal18kEscPlaceholderFormat;
        private SmartFormatter _literalFormatter;

        public FormatTests()
        {
            _placeholderFormat = _parser.ParseFormat("{0}");
            _placeholder0005Format = _parser.ParseFormat("{0}{1}{2}{3}{4}");
            _literal0010CharFormat = _parser.ParseFormat("1234567890");
            _literal3000CharFormat = _parser.ParseFormat(LoremIpsum);
            _literal6000PlaceholderFormat = _parser.ParseFormat(LoremIpsum + "{0}" + LoremIpsum);
            _literal18kEscPlaceholderFormat = _parser.ParseFormat(LoremIpsum + @"\n" + LoremIpsum + "{0}" + LoremIpsum);

            _literalFormatter = new SmartFormatter();
            _literalFormatter.AddExtensions(
                new DefaultSource()
            );
            _literalFormatter.AddExtensions(
                new DefaultFormatter()
            );
        }

        public ZStringOutput GetOutput(Format format)
        {
            // Note: a good estimation of the expected output length is essential for performance and GC pressure
            // return new StringOutput(format.Length + format.Items.Count * 8);
            // return new NullOutput()
            return new ZStringOutput(format.Length + format.Items.Count * 8);
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
                 using var output = GetOutput(_placeholderFormat);
                _literalFormatter.FormatInto(output, _placeholderFormat, LoremIpsum);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Placeholder0005()
        {
            for (var i = 0; i < N; i++)
            {
                using var output = GetOutput(_placeholder0005Format);
                _literalFormatter.FormatInto(output, _placeholder0005Format, LoremIpsum, LoremIpsum, LoremIpsum, LoremIpsum, LoremIpsum);
                _ = output.ToString();
            }
        }

        [Benchmark(Baseline = true)]
        public void Literal0010Char()
        {
            for (var i = 0; i < N; i++)
            {
                using var output = GetOutput(_literal0010CharFormat);
                _literalFormatter.FormatInto(output, _literal0010CharFormat);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Literal3000Char()
        {
            for (var i = 0; i < N; i++)
            {
                using var output = GetOutput(_literal3000CharFormat);
                _literalFormatter.FormatInto(output, _literal3000CharFormat);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Literal6000Placeholder()
        {
            for (var i = 0; i < N; i++)
            {
                using var output = GetOutput(_literal6000PlaceholderFormat);
                _literalFormatter.FormatInto(output, _literal6000PlaceholderFormat, LoremIpsum);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Literal18kUniPlaceholder()
        {
            for (var i = 0; i < N; i++)
            {
                using var output = GetOutput(_literal18kEscPlaceholderFormat);
                _literalFormatter.FormatInto(output, _literal18kEscPlaceholderFormat, LoremIpsum);
                _ = output.ToString();
            }
        }
    }
}
