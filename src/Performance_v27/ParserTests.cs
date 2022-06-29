using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Performance
{
/*
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1165 (21H1/May2021Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=5.0.302
  [Host]   : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  .NET 5.0 : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT

Job=.NET 5.0  Runtime=.NET 5.0

|                   Method |    N |         Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------- |----- |-------------:|-----------:|-----------:|------:|--------:|---------:|-------:|------:|----------:|
|              Placeholder |   10 |     3.133 us |  0.0357 us |  0.0334 us |  1.57 |    0.02 |   1.2512 |      - |     - |     10 KB |
|          Placeholder0005 |   10 |    10.439 us |  0.0358 us |  0.0318 us |  5.24 |    0.06 |   2.4261 | 0.0153 |     - |     20 KB |
| PlaceholderFormatter0002 |   10 |     8.538 us |  0.0664 us |  0.0621 us |  4.29 |    0.06 |   2.1362 |      - |     - |     18 KB |
|          Literal0010Char |   10 |     1.992 us |  0.0229 us |  0.0214 us |  1.00 |    0.00 |   1.0300 |      - |     - |      8 KB |
|          Literal3000Char |   10 |    68.469 us |  0.2249 us |  0.1878 us | 34.31 |    0.37 |   0.9766 |      - |     - |      8 KB |
|   Literal3000Placeholder |   10 |    69.836 us |  0.1300 us |  0.1086 us | 35.00 |    0.35 |   1.2207 |      - |     - |     11 KB |
|                          |      |              |            |            |       |         |          |        |       |           |
|              Placeholder | 1000 |   331.250 us |  2.5982 us |  2.3032 us |  1.70 |    0.02 | 125.0000 |      - |     - |  1,023 KB |
|          Placeholder0005 | 1000 | 1,090.455 us |  7.6007 us |  7.1097 us |  5.59 |    0.07 | 242.1875 |      - |     - |  1,984 KB |
| PlaceholderFormatter0002 | 1000 |   851.503 us |  3.0664 us |  2.7183 us |  4.36 |    0.03 | 213.8672 | 0.9766 |     - |  1,750 KB |
|          Literal0010Char | 1000 |   195.187 us |  1.7088 us |  1.5148 us |  1.00 |    0.00 | 103.2715 |      - |     - |    844 KB |
|          Literal3000Char | 1000 | 6,836.514 us | 21.9338 us | 19.4437 us | 35.03 |    0.29 | 101.5625 |      - |     - |    844 KB |
|   Literal3000Placeholder | 1000 | 6,992.903 us | 15.9526 us | 14.1415 us | 35.83 |    0.27 | 125.0000 |      - |     - |  1,062 KB | 
*/

    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class ParserTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divide";

        private Parser _parser;
        private readonly string[] _formatterNames = Smart.Default.GetNotEmptyFormatterExtensionNames();

        public ParserTests()
        {
            _parser = new SmartFormatter().Parser;
            _parser.UseAlternativeEscapeChar();
            _parser.AddAlphanumericSelectors();
            _parser.AddAdditionalSelectorChars("_-");
            _parser.AddOperators(".?,[]");
        }

        [Params(10, 1000)]
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
                _ = _parser.ParseFormat("{SomePlaceholder}", _formatterNames);
            }
        }

        [Benchmark]
        public void Placeholder0005()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _parser.ParseFormat("{SomePlaceholder1}{SomePlaceholder2}{SomePlaceholder3}{SomePlaceholder4}{SomePlaceholder5}", _formatterNames);
            }
        }

        [Benchmark]
        public void PlaceholderFormatter0002()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _parser.ParseFormat("{SomePlaceholder1:choose:}{SomePlaceholder2:list:}", _formatterNames);
            }
        }

        [Benchmark(Baseline = true)]
        public void Literal0010Char()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _parser.ParseFormat("1234567890", _formatterNames);
            }
        }

        [Benchmark]
        public void Literal3000Char()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _parser.ParseFormat(LoremIpsum, _formatterNames);
            }
        }

        [Benchmark]
        public void Literal3000Placeholder()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _parser.ParseFormat(LoremIpsum + "{SomePlaceholder}", _formatterNames);
            }
        }
    }
}
