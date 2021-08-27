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
 BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1165 (21H1/May2021Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=5.0.302
  [Host]   : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT
  .NET 5.0 : .NET 5.0.8 (5.0.821.31504), X64 RyuJIT

Job=.NET 5.0  Runtime=.NET 5.0

|                   Method |    N |         Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |----- |-------------:|-----------:|-----------:|------:|--------:|---------:|------:|------:|----------:|
|              Placeholder |   10 |     4.027 us |  0.0308 us |  0.0240 us |  3.98 |    0.04 |   0.6638 |     - |     - |      5 KB |
|          Placeholder0005 |   10 |    18.131 us |  0.0702 us |  0.0656 us | 17.93 |    0.11 |   1.8311 |     - |     - |     15 KB |
| PlaceholderFormatter0002 |   10 |     9.211 us |  0.0916 us |  0.0812 us |  9.11 |    0.08 |   1.1749 |     - |     - |     10 KB |
|          Literal0010Char |   10 |     1.011 us |  0.0062 us |  0.0058 us |  1.00 |    0.00 |   0.4482 |     - |     - |      4 KB |
|          Literal3000Char |   10 |    67.525 us |  0.1731 us |  0.1619 us | 66.77 |    0.46 |   0.3662 |     - |     - |      4 KB |
|   Literal3000Placeholder |   10 |    76.442 us |  0.1780 us |  0.1665 us | 75.58 |    0.49 |   0.6104 |     - |     - |      6 KB |
|                          |      |              |            |            |       |         |          |       |       |           |
|              Placeholder | 1000 |   376.009 us |  2.9151 us |  2.7268 us |  3.77 |    0.03 |  66.8945 |     - |     - |    547 KB |
|          Placeholder0005 | 1000 | 1,796.686 us |  9.3210 us |  8.7189 us | 18.03 |    0.12 | 183.5938 |     - |     - |  1,508 KB |
| PlaceholderFormatter0002 | 1000 |   865.751 us |  2.7868 us |  2.4704 us |  8.68 |    0.07 | 118.1641 |     - |     - |    969 KB |
|          Literal0010Char | 1000 |    99.653 us |  0.6222 us |  0.5820 us |  1.00 |    0.00 |  44.9219 |     - |     - |    367 KB |
|          Literal3000Char | 1000 | 6,757.903 us | 13.6353 us | 12.0874 us | 67.79 |    0.43 |  39.0625 |     - |     - |    367 KB |
|   Literal3000Placeholder | 1000 | 7,097.778 us | 16.7876 us | 14.0184 us | 71.20 |    0.36 |  70.3125 |     - |     - |    586 KB |
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
                _literalFormatter.FormatInto(GetOutput(_placeholderFormat.Length + _placeholderFormat.Items.Count * 8), _placeholderFormat, "ph1");
            }
        }

        [Benchmark]
        public void Placeholder0005()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatInto(GetOutput(_placeholder0005Format.Length  + _placeholder0005Format.Items.Count * 8), _placeholder0005Format, "ph1", "ph2", "ph3", "ph4", "ph5");
            }
        }

        [Benchmark(Baseline = true)]
        public void Literal0010Char()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatInto(GetOutput(_literal0010CharFormat.Length + _literal0010CharFormat.Items.Count * 8), _literal0010CharFormat);
            }
        }

        [Benchmark]
        public void Literal3000Char()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatInto(GetOutput(_literal3000CharFormat.Length + _literal3000CharFormat.Items.Count * 8), _literal3000CharFormat);
            }
        }

        [Benchmark]
        public void Literal6000Placeholder()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatInto(GetOutput(_literal6000PlaceholderFormat.Length + _literal6000PlaceholderFormat.Items.Count * 8), _literal6000PlaceholderFormat, "ph1");
            }
        }

        [Benchmark]
        public void Literal18kUniPlaceholder()
        {
            for (var i = 0; i < N; i++)
            {
                _literalFormatter.FormatInto(GetOutput(_literal18kEscPlaceholderFormat.Length + _literal18kEscPlaceholderFormat.Items.Count * 8), _literal6000PlaceholderFormat, "ph1");
            }
        }
    }
}
