using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Pooling;
using SmartFormat.Pooling.ObjectPools;

namespace SmartFormat.Tests.Pooling
{
    [TestFixture]
    public class ConcurrentPoolingTests
    {
        private static void ResetAllPools(bool goThreadSafe)
        {
            // get specialized pools (includes smart pools)
            foreach (dynamic p in PoolRegistry.Items.Values)
            {
                p.Reset(goThreadSafe);
            }
        }

        private static List<(Type? Type, IPoolCounters? Counters)> GetAllPoolCounters()
        {
            var l = new List<(Type? Type, IPoolCounters? Counters)>();

            foreach (dynamic p in PoolRegistry.Items.Values)
            {
                if (p.Pool is IPoolCounters counters)
                    l.Add((p.GetType(), counters));
            }

            return l;
        }

        [Test]
        public void Parallel_Load_On_Pool()
        {
            var policy = new PoolPolicy<ObjectPoolClassesTests.SomePoolObject>
            {
                FunctionOnCreate = () => new ObjectPoolClassesTests.SomePoolObject { Value = "created" },
                ActionOnGet = o => o.Value = "get",
                ActionOnReturn = o => o.Value = "returned",
                ActionOnDestroy = o => o.Value = "destroyed",
                MaximumPoolSize = 100,
                InitialPoolSize = 1
            };

            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            var pool = new ObjectPoolConcurrent<ObjectPoolClassesTests.SomePoolObject>(policy) { IsPoolingEnabled = true };

            Assert.That(() =>
                Parallel.For(0L, 1000, options, (i, loopState) =>
                {
                    var someObject = pool.Get();
                    pool.Return(someObject);
                }), Throws.Nothing);
            Assert.That(pool.CountActive, Is.EqualTo(0));
            Assert.That(pool.CountInactive, Is.GreaterThan(0));
        }

        [Test]
        public void Parallel_Load_On_Specialized_Pools()
        {
            // Switch to thread safety
            const bool currentThreadSafeMode = true;
            var savedIsThreadSafeMode = SmartSettings.IsThreadSafeMode;
            SmartSettings.IsThreadSafeMode = currentThreadSafeMode;
            ResetAllPools(currentThreadSafeMode);

            const int maxLoops = 100;
            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            SmartSettings.IsThreadSafeMode = true;
            var list = new ConcurrentBag<string>();

            Assert.That(() =>
                Parallel.For(1L, maxLoops, options, (i, loopState) =>
                {
                    using var formatParsed = new Parser().ParseFormat("Number: {0:00000}");
                    var smart = new SmartFormatter();
                    smart.AddExtensions(new DefaultSource());
                    smart.AddExtensions(new DefaultFormatter());
                    list.Add(smart.Format(formatParsed, i));
                }), Throws.Nothing);

            var result = list.OrderBy(e => e);
            long compareCounter = 1;
            
            Assert.That(list.Count, Is.EqualTo(maxLoops - 1));
            Assert.That(result.All(r => r == $"Number: {compareCounter++:00000}"));

            foreach (var p in GetAllPoolCounters())
            {
                if (p.Counters!.CountAll <= 0) continue;

                Console.WriteLine(p.Type + @":");
                Console.WriteLine(@"{0}: {1}", nameof(IPoolCounters.CountActive), p.Counters?.CountActive);
                Console.WriteLine(@"{0}: {1}", nameof(IPoolCounters.CountInactive), p.Counters?.CountInactive);

                Console.WriteLine();
                Assert.That(p.Counters!.CountActive, Is.EqualTo(0), string.Join(" ", nameof(IPoolCounters.CountActive), p.Type?.ToString()));
                Assert.That(p.Counters.CountInactive, Is.GreaterThan(0), string.Join(" ", nameof(IPoolCounters.CountInactive), p.Type?.ToString()));
            }

            // Restore thread safety
            SmartSettings.IsThreadSafeMode = savedIsThreadSafeMode;
            ResetAllPools(savedIsThreadSafeMode);

            Assert.That(PoolRegistry.Items.Count, Is.EqualTo(0), "PoolRegistry.Items");
        }
    }
}
