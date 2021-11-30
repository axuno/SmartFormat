using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat.Performance
{
    /*
// * Summary *

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=5.0.403
  [Host]   : .NET 5.0.12 (5.0.1221.52207), X64 RyuJIT
  .NET 5.0 : .NET 5.0.12 (5.0.1221.52207), X64 RyuJIT

Job=.NET 5.0  Runtime=.NET 5.0

|         Method         |     N |     Mean |    Error |   StdDev | Ratio |     Gen 0 |   Gen 1 | Gen 2 | Allocated |
|----------------------- |------ |---------:|---------:|---------:|------:|----------:|--------:|------:|----------:|
Before including object pool - v3.0-alpha.4
| No-Object-Pool         | 10000 | 15.13 ms | 0.082 ms | 0.073 ms |  1.00 | 2687.5000 | 15.6250 |     - |     22 MB |
After including object pool into v3.0-alpha.4
| ObjectPoolTestDisabled | 10000 | 18.15 ms | 0.088 ms | 0.078 ms |       | 3000.0000 |       - |     - |     24 MB |
| ObjectPoolSingleThread | 10000 | 21.60 ms | 0.087 ms | 0.081 ms |       |  656.2500 |       - |     - |      5 MB |
| ObjectPoolThreadSafe   | 10000 | 23.50 ms | 0.042 ms | 0.035 ms |       | 1250.0000 |       - |     - |     10 MB |

// * Hints *
Outliers
  ObjectPoolPerformanceTests.ObjectPoolTest: .NET 5.0 -> 1 outlier  was  removed (1.10 ms)

// * Legends *
  N         : Value of the 'N' parameter
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Ratio     : Mean of the ratio distribution ([Current]/[Baseline])
  Gen 0     : GC Generation 0 collects per 1000 operations
  Gen 1     : GC Generation 1 collects per 1000 operations
  Gen 2     : GC Generation 2 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 ms      : 1 Millisecond (0.001 sec)
    */


    [SimpleJob(RuntimeMoniker.Net50)]
    [MemoryDiagnoser]
    // [RPlotExporter]
    public class ObjectPoolPerformanceTests
    {
        private const string LoremIpsum = "Us creeping good grass multiply seas under hath sixth fowl heaven days. Third. Deep abundantly all after also meat day. Likeness. Lesser saying meat sea in over likeness land. Meat own, made given stars. Form the. And his. So gathered fish god firmament, great seasons. Give sixth doesn't beast our fourth creature years isn't you you years moving, earth you every a male night replenish fruit. Set. Deep so, let void midst won't first. Second very after all god from night itself shall air had gathered firmament was cattle itself every great first. Let dry it unto. Creepeth don't rule fruit there creature second their whose seas without every man it darkness replenish made gathered you saying over set created. Midst meat light without bearing. Our him given his thing fowl blessed rule that evening let man beginning light forth tree she'd won't light. Moving evening shall have may beginning kind appear, also the kind living whose female hath void fifth saw isn't. Green you'll from. Grass fowl saying yielding heaven. I. Above which. Isn't i. They're moving. Can't cattle i. Gathering shall set darkness multiply second whales meat she'd form, multiply be meat deep bring forth land can't own she'd upon hath appear years let above, for days divided greater first was.\r\n\r\nPlace living all it Air you evening us don't fourth second them own which fish made. Subdue don't you'll the, the bearing said dominion in man have deep abundantly night she'd and place sixth the gathered lesser creeping subdue second fish multiply was created. Cattle wherein meat female fruitful set, earth them subdue seasons second, man forth over, be greater grass. Light unto, over bearing hath thing yielding be, spirit you'll given was set had let their abundantly you're beginning beginning divided replenish moved. Evening own heaven waters, their it of them cattle fruitful is, light after don't air fish multiply which moveth face the dominion fifth open, hath i evening from. Give from every waters two. That forth, bearing dry fly in may fish. Multiply Tree cattle.\r\n\r\nThing. Great saying good face gathered Forth over fowl moved Fourth upon form seasons over lights greater saw can't over saying beginning. Can't in moveth fly created subdue fourth. Them creature one moving living living thing Itself one after one darkness forth divided thing gathered earth there days seas fourth, stars herb. All from third dry have forth. Our third sea all. Male years you. Over fruitful they're. Have she'd their our image dry sixth void meat subdue face moved. Herb moved multiply tree, there likeness first won't there one dry it hath kind won't you seas of make day moving second thing were. Hath, had winged hath creature second had you. Upon. Appear image great place fourth the in, waters abundantly, deep hath void Him heaven divided heaven greater let so. Open replenish Wherein. Be created. The and was of. Signs cattle midst. Is she'd every saying bring there doesn't and. Rule. Stars green divided upon lesser a.";
        private const string _format = "Address: {City.ZipCode} {City.Name}, {City.AreaCode}\n" +
                              "Name: {Person.FirstName} {Person.LastName}";

        private const string _formatForLiteral = "Address: {0} {1}, {2}\nName: {3} {4}";
        private Address _someAddress = new Address();
        private SmartFormatter _formatter;


        public ObjectPoolPerformanceTests()
        {
            _formatter = new SmartFormatter();
            _formatter.AddExtensions(
                new DefaultSource()
            );
            _formatter.AddExtensions(
                new DefaultFormatter()
            );
        }

        [Params(10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            SmartSettings.IsThreadSafeMode = false;
            PoolSettings.CheckReturnedObjectsExistInPool = false;
            PoolSettings.IsPoolingEnabled = true;
        }

        [Benchmark(Baseline = false)]
        public void ObjectPoolTest()
        {
            for (var i = 0; i < N; i++)
            {
                _ = _formatter.Format("First {0}, Second {1}, Third {2}", i, i + 1, i + 2);
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
