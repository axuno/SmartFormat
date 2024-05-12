using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Extensions;

namespace SmartFormat.Performance
{
/*
StringOutput v2.7
|                   Method |     N |         Mean |      Error |       StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------- |------ |-------------:|-----------:|-------------:|------:|--------:|-----------:|-----------:|------:|-----------:|
|              Placeholder |   100 |     94.95 us |   1.865 us |     3.637 us |  8.99 |    0.51 |   148.4375 |     3.4180 |     - |   1,215 KB |
|          Placeholder0005 |   100 |    496.86 us |  15.647 us |    45.145 us | 50.65 |    3.92 |   845.7031 |   101.0742 |     - |   6,930 KB |
|          Literal0010Char |   100 |     10.37 us |   0.199 us |     0.245 us |  1.00 |    0.00 |     3.9215 |          - |     - |      32 KB |
|          Literal3000Char |   100 |     64.47 us |   1.204 us |     1.126 us |  6.21 |    0.23 |   146.1182 |     3.1738 |     - |   1,196 KB |
|   Literal6000Placeholder |   100 |    295.81 us |   2.926 us |     2.737 us | 28.47 |    0.87 |   648.9258 |    53.7109 |     - |   5,320 KB |
| Literal18kEscPlaceholder |   100 |    391.78 us |   3.079 us |     2.880 us | 37.70 |    1.17 |   913.0859 |   101.0742 |     - |   7,473 KB |
|                          |       |              |            |              |       |         |            |            |       |            |
|              Placeholder | 10000 | 10,758.24 us | 159.495 us |   149.192 us | 10.30 |    0.22 | 14843.7500 |   343.7500 |     - | 121,484 KB |
|          Placeholder0005 | 10000 | 47,659.35 us | 907.987 us | 1,080.894 us | 45.90 |    1.30 | 84600.0000 | 10100.0000 |     - | 692,969 KB |
|          Literal0010Char | 10000 |  1,042.43 us |  18.413 us |    16.323 us |  1.00 |    0.00 |   390.6250 |          - |     - |   3,203 KB |
|          Literal3000Char | 10000 |  6,227.31 us | 120.366 us |   138.613 us |  5.95 |    0.16 | 14617.1875 |   320.3125 |     - | 119,609 KB |
|   Literal6000Placeholder | 10000 | 27,852.48 us | 553.720 us | 1,561.776 us | 26.30 |    0.81 | 64906.2500 |  5406.2500 |     - | 532,031 KB |
| Literal18kEscPlaceholder | 10000 | 38,686.09 us | 757.920 us |   671.876 us | 37.12 |    0.93 | 91285.7143 | 10071.4286 |     - | 747,344 KB |

NullOutput v2.7
|                   Method |     N |          Mean |       Error |      StdDev | Ratio | RatioSD |      Gen 0 |   Gen 1 | Gen 2 |  Allocated |
|------------------------- |------ |--------------:|------------:|------------:|------:|--------:|-----------:|--------:|------:|-----------:|
|              Placeholder |   100 |     25.964 us |   0.5008 us |   0.5767 us |  4.28 |    0.14 |     3.5095 |       - |     - |      29 KB |
|          Placeholder0005 |   100 |    108.185 us |   1.1526 us |   0.9625 us | 17.73 |    0.37 |     8.4229 |       - |     - |      70 KB |
|          Literal0010Char |   100 |      6.064 us |   0.1175 us |   0.1354 us |  1.00 |    0.00 |     2.0065 |       - |     - |      16 KB |
|          Literal3000Char |   100 |      5.933 us |   0.1175 us |   0.1759 us |  0.99 |    0.03 |     2.0065 |       - |     - |      16 KB |
|   Literal6000Placeholder |   100 |     88.677 us |   0.7209 us |   0.6390 us | 14.54 |    0.30 |   147.0947 |       - |     - |   1,202 KB |
| Literal18kEscPlaceholder |   100 |    122.497 us |   0.5515 us |   0.5159 us | 20.07 |    0.43 |   219.6045 |  0.1221 |     - |   1,795 KB |
|                          |       |               |             |             |       |         |            |         |       |            |
|              Placeholder | 10000 |  2,550.337 us |  26.7585 us |  23.7207 us |  4.54 |    0.06 |   351.5625 |       - |     - |   2,891 KB |
|          Placeholder0005 | 10000 | 10,847.854 us | 183.6711 us | 171.8061 us | 19.26 |    0.39 |   843.7500 |       - |     - |   6,953 KB |
|          Literal0010Char | 10000 |    563.425 us |   8.8864 us |   8.3123 us |  1.00 |    0.00 |   200.1953 |       - |     - |   1,641 KB |
|          Literal3000Char | 10000 |    585.451 us |   6.9466 us |   6.4979 us |  1.04 |    0.02 |   200.1953 |       - |     - |   1,641 KB |
|   Literal6000Placeholder | 10000 |  9,179.534 us | 178.6525 us | 272.8212 us | 16.01 |    0.54 | 14703.1250 |       - |     - | 120,234 KB |
| Literal18kEscPlaceholder | 10000 | 13,271.904 us |  42.8786 us |  40.1087 us | 23.56 |    0.35 | 21953.1250 | 15.6250 |     - | 179,453 KB |

Smart.Format 3.0.0-alpha.4 plus Object Pool
|                   Method |     N |         Mean |      Error |     StdDev | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |  Allocated |
|------------------------- |------ |-------------:|-----------:|-----------:|------:|--------:|-----------:|------:|------:|-----------:|
|              Placeholder |   100 |     72.14 us |   0.811 us |   0.759 us |  3.83 |    0.07 |    72.5098 |     - |     - |     593 KB |
|          Placeholder0005 |   100 |    291.37 us |   3.793 us |   3.548 us | 15.47 |    0.25 |   358.3984 |     - |     - |   2,934 KB |
|          Literal0010Char |   100 |     18.84 us |   0.227 us |   0.212 us |  1.00 |    0.00 |     0.9460 |     - |     - |       8 KB |
|          Literal3000Char |   100 |     48.00 us |   0.326 us |   0.305 us |  2.55 |    0.04 |    72.1436 |     - |     - |     590 KB |
|   Literal6000Placeholder |   100 |    153.43 us |   0.641 us |   0.599 us |  8.15 |    0.12 |   214.8438 |     - |     - |   1,762 KB |
| Literal18kUniPlaceholder |   100 |    198.06 us |   0.767 us |   0.718 us | 10.52 |    0.12 |   286.3770 |     - |     - |   2,349 KB |
|                          |       |              |            |            |       |         |            |       |       |            |
|              Placeholder | 10000 |  7,313.00 us |  43.041 us |  38.154 us |  3.86 |    0.04 |  7250.0000 |     - |     - |  59,297 KB |
|          Placeholder0005 | 10000 | 29,257.10 us | 139.998 us | 130.955 us | 15.44 |    0.23 | 35812.5000 |     - |     - | 293,359 KB |
|          Literal0010Char | 10000 |  1,894.69 us |  24.862 us |  23.256 us |  1.00 |    0.00 |    93.7500 |     - |     - |     781 KB |
|          Literal3000Char | 10000 |  4,765.76 us |  21.656 us |  20.257 us |  2.52 |    0.04 |  7218.7500 |     - |     - |  58,984 KB |
|   Literal6000Placeholder | 10000 | 15,224.47 us |  52.517 us |  46.555 us |  8.04 |    0.10 | 21500.0000 |     - |     - | 176,172 KB |
| Literal18kUniPlaceholder | 10000 | 19,875.30 us |  55.815 us |  49.479 us | 10.50 |    0.14 | 28625.0000 |     - |     - | 234,922 KB |

*/

    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class FormatTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divide";

        private readonly Parser _parser = new SmartFormatter {Settings = {ConvertCharacterStringLiterals = true}}.Parser;
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

        public StringOutput GetOutput(Format format)
        {
            // Note: a good estimation of the expected output length is essential for performance and GC pressure
            //return new NullOutput();
            return new StringOutput(format.baseString.Length + format.Items.Count * 8);
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
                var output = GetOutput(_placeholderFormat.Format);
                _literalFormatter.FormatWithCacheInto(ref _placeholderFormat, output, string.Empty, LoremIpsum );
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Placeholder0005()
        {
            for (var i = 0; i < N; i++)
            {
                var output = GetOutput(_placeholder0005Format.Format);
                _literalFormatter.FormatWithCacheInto(ref _placeholder0005Format, output, string.Empty, LoremIpsum, LoremIpsum, LoremIpsum, LoremIpsum, LoremIpsum);
                _ = output.ToString();
            }
        }

        [Benchmark(Baseline = true)]
        public void Literal0010Char()
        {
            for (var i = 0; i < N; i++)
            {
                var output = GetOutput(_literal0010CharFormat.Format);
                _literalFormatter.FormatWithCacheInto(ref _literal0010CharFormat, output, string.Empty);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Literal3000Char()
        {
            for (var i = 0; i < N; i++)
            {
                var output = GetOutput(_literal3000CharFormat.Format);
                _literalFormatter.FormatWithCacheInto(ref _literal3000CharFormat, output, string.Empty);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Literal6000Placeholder()
        {
            for (var i = 0; i < N; i++)
            {
                var output = GetOutput(_literal6000PlaceholderFormat.Format);
                _literalFormatter.FormatWithCacheInto(ref _literal6000PlaceholderFormat, output, string.Empty, LoremIpsum);
                _ = output.ToString();
            }
        }

        [Benchmark]
        public void Literal18kEscPlaceholder()
        {
            for (var i = 0; i < N; i++)
            {
                var output = GetOutput(_literal18kEscPlaceholderFormat.Format);
                _literalFormatter.FormatWithCacheInto(ref _literal18kEscPlaceholderFormat, output, string.Empty, LoremIpsum);
                _ = output.ToString();
            }
        }
    }
}
