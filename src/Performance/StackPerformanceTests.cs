using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SmartFormat.Performance
{
    /*
// * Summary *

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.1288 (20H2/October2020Update)
Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.403
  [Host]   : .NET 5.0.12 (5.0.1221.52207), X64 RyuJIT
  .NET 5.0 : .NET 5.0.12 (5.0.1221.52207), X64 RyuJIT

Job=.NET 5.0  Runtime=.NET 5.0

|                   Method |     N |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------- |------ |-----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|        SingleThreadStack |  1000 |   6.506 us | 0.0460 us | 0.0408 us |  1.00 |    0.00 |       - |      - |     - |         - |
| SingleThreadStackWithCnt |  1000 |   7.109 us | 0.0955 us | 0.0797 us |  1.09 |    0.01 |       - |      - |     - |         - |
|          ConcurrentStack |  1000 |  22.692 us | 0.3551 us | 0.3148 us |  3.49 |    0.05 |  7.6294 | 0.0305 |     - |  32,000 B |
|   ConcurrentStackWithCnt |  1000 |  31.143 us | 0.4926 us | 0.4608 us |  4.79 |    0.08 |  7.6294 | 0.0610 |     - |  32,000 B |
|                          |       |            |           |           |       |         |         |        |       |           |
|        SingleThreadStack | 10000 |  67.166 us | 0.5042 us | 0.4469 us |  1.00 |    0.00 |       - |      - |     - |         - |
| SingleThreadStackWithCnt | 10000 |  72.540 us | 0.9926 us | 0.9285 us |  1.08 |    0.02 |       - |      - |     - |         - |
|          ConcurrentStack | 10000 | 239.337 us | 1.6540 us | 1.4662 us |  3.56 |    0.04 | 74.2188 | 8.5449 |     - | 320,000 B |
|   ConcurrentStackWithCnt | 10000 | 321.594 us | 1.0934 us | 0.9692 us |  4.79 |    0.04 | 74.7070 | 7.8125 |     - | 320,000 B |

// * Hints *
Outliers
  StackPerformanceTests.SingleThreadStack: .NET 5.0        -> 1 outlier  was  removed, 3 outliers were detected (6.42 us, 6.42 us, 6.73 us)
  StackPerformanceTests.SingleThreadStackWithCnt: .NET 5.0 -> 2 outliers were removed (7.36 us, 7.40 us)
  StackPerformanceTests.ConcurrentStack: .NET 5.0          -> 1 outlier  was  removed (35.98 us)
  StackPerformanceTests.SingleThreadStack: .NET 5.0        -> 1 outlier  was  removed (68.41 us)
  StackPerformanceTests.ConcurrentStack: .NET 5.0          -> 1 outlier  was  removed, 3 outliers were detected (236.86 us, 236.95 us, 243.70 us)
  StackPerformanceTests.ConcurrentStackWithCnt: .NET 5.0   -> 1 outlier  was  removed (328.96 us)

// * Legends *
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
    */
    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class StackPerformanceTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divided upon lesser a.";
        private const string Format = "Address: {City.ZipCode} {City.Name}, {City.AreaCode}\n" +
                              "Name: {Person.FirstName} {Person.LastName}";

        private const string FormatForLiteral = "Address: {0} {1}, {2}\nName: {3} {4}";

        private readonly Stack<Address> _singleStack = new Stack<Address>();
        private readonly ConcurrentStack<Address> _concStack = new ConcurrentStack<Address>();
        private readonly Address _someAddress = new Address();
        private int _counter;

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark(Baseline = true)]
        public void SingleThreadStack()
        {
            for (var i = 0; i < N; i++)
            {
                _singleStack.Push(_someAddress);
            }
            for (var i = 0; i < N; i++)
            {
                _ = _singleStack.Pop();
            }
        }

        [Benchmark]
        public void SingleThreadStackWithCnt()
        {
            for (var i = 0; i < N; i++)
            {
                _singleStack.Push(_someAddress);
                _counter++;
            }
            for (var i = 0; i < N; i++)
            {
                _ = _singleStack.Pop();
                _counter--;
            }
        }

        [Benchmark]
        public void ConcurrentStack()
        {
            for (var i = 0; i < N; i++)
            {
                _concStack.Push(_someAddress);
            }
            for (var i = 0; i < N; i++)
            {
                _ = _concStack.TryPop(out _);
            }
        }

        [Benchmark]
        public void ConcurrentStackWithCnt()
        {
            for (var i = 0; i < N; i++)
            {
                _concStack.Push(_someAddress);
                Interlocked.Increment(ref _counter);
            }
            for (var i = 0; i < N; i++)
            {
                _ = _concStack.TryPop(out _);
                Interlocked.Decrement(ref _counter);
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
