using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Performance
{
    /*
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET Core SDK=5.0.202
  [Host]        : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=.NET Core 5.0  Runtime=.NET Core 5.0

|            Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
|  SmartFormat2.7.0 | 79.23 us | 0.541 us | 0.479 us | 0.8545 |     - |     - |   7.84 KB |
|  ParseSmartFormat | 81.93 us | 1.035 us | 0.968 us | 1.0986 |     - |     - |   9.87 KB |
| ParsePlaceholders | 38.14 us | 0.249 us | 0.220 us | 0.1831 |     - |     - |   1.53 KB |
    */

    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    public class SimpleSpanParserTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divided upon lesser a.";
        private ReadOnlyMemory<char> _inputFormatMemory;
        private string _inputFormatString;
        private Parser _sfParser;
        
        [GlobalSetup]
        public void Setup()
        {
            var ctorSettings = typeof(SmartSettings).GetConstructors
                (BindingFlags.Instance | BindingFlags.NonPublic)[0];

            var ctorParser = typeof(Parser).GetConstructors
                (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];

            var settings = (SmartSettings) ctorSettings.Invoke(null);
            _sfParser = (Parser) ctorParser.Invoke(new object[] { settings });

            for (int i = 0; i < 10; i++)
            {
                _inputFormatString += @"This {1} is the input {2} \{ format \}" + LoremIpsum;
            }

            _inputFormatMemory = _inputFormatString.AsMemory();
        }

        [Benchmark]
        public void ParseSmartFormat()
        {
            var result = _sfParser.ParseFormat(_inputFormatString, new[] {"default"});
        }

        [Benchmark]
        public void ParsePlaceholders()
        {
            
            var placeholders = new List<(int StartIndex, ReadOnlyMemory<char> Content)>();
            
            var length = _inputFormatMemory.Length;
            int level = 0, start = 0;
            // Using the ReadOnlyMemory.Span doubled speed
            var inputFormatSpan = _inputFormatMemory.Span;
            for (var index = 0; index < length; index++)
            {
                var currentChar = inputFormatSpan[index];
                if (currentChar == '\\' && index + 1 < length)
                {
                    var nextChar = inputFormatSpan[index + 1];
                    if (nextChar == '{' || nextChar == '}') index++;
                }

                if (currentChar == '{' && index + 1 < length) {
                    if(level == 0) start = index;
                    level++;
                }

                if (currentChar == '}')
                {
                    level--;
                    if (level == 0)
                    {
                        placeholders.Add((start, _inputFormatMemory.Slice(start, index + 1 - start)));
                    }
                }
            }
            /*
            // Print literals and placeholders
            var lastStartPos = 0;
            foreach (var ph in placeholders)
            {
                Console.WriteLine("==>" + inputFormat.Slice(lastStartPos, ph.Start - lastStartPos) + "<==");
                lastStartPos = ph.Start + ph.Slice.Length;
                Console.WriteLine("==>" + ph.Slice.ToString() + "<==");
            }
            Console.WriteLine("==>" + inputFormat.Slice(lastStartPos) + "<==");
            if (level != 0) Console.WriteLine("Unbalanced braces");
            */
        }
    }
}