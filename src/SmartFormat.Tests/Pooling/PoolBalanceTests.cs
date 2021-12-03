using System;
using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Pooling;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SmartPools;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Pooling
{
    [TestFixture]
    public class PoolBalanceTests
    {
        private static List<(Type? Type, IPoolCounters? Counters)> GetAllSmartPoolsCleared()
        {
            var l = new List<(Type? Type, IPoolCounters? Counters)>();

            // get smart pools
            var smartPoolTypes = ReflectionTools.GetSubclassesOf(typeof(SmartPoolAbstract<>).Assembly, typeof(SmartPoolAbstract<>));

            foreach (var poolType in smartPoolTypes)
            {
                dynamic? instance = poolType.GetProperty("Instance")?.GetValue(null, null);
                instance?.Pool.Clear();
                l.Add((poolType, instance?.Pool as IPoolCounters));
            }

            return l;
        }

        [Test]
        public void Trying_To_Return_Initialization_Object_Should_Throw()
        {
            Assert.That(code: () => FormatDetailsPool.Instance.Return(InitializationObject.FormatDetails), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => FormattingInfoPool.Instance.Return(InitializationObject.FormattingInfo), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => FormatPool.Instance.Return(InitializationObject.Format), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => LiteralTextPool.Instance.Return(InitializationObject.LiteralText), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => ParsingErrorsPool.Instance.Return(InitializationObject.ParsingErrors), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => PlaceholderPool.Instance.Return(InitializationObject.Placeholder), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => SelectorPool.Instance.Return(InitializationObject.Selector), Throws.InstanceOf<PoolingException>());
            Assert.That(code: () => SplitListPool.Instance.Return(InitializationObject.SplitList), Throws.InstanceOf<PoolingException>());
        }

        private static void DoSomeFormattingWork()
        {
            const string indexPlaceholders = "All items: {0[0]}, {0[1]}, and {0[2]}";
            const string listPlaceholders = "Total items: {0.Count}. All items: {0:list:{}|, |, and }";
            var list = new List<int> { 1, 2, 3 };
            
            var smart = new SmartFormatter();
            var listSourceAndFormat = new ListFormatter();
            smart.AddExtensions(listSourceAndFormat, new StringSource(), new ReflectionSource(), new DefaultSource());
            smart.AddExtensions(listSourceAndFormat, new DefaultFormatter());

            // Do some formatting work
            for (var i = 0; i < 4; i++)
            {
                // Parse and format
                _ = Smart.Format(indexPlaceholders, list);
                _ = Smart.Format(listPlaceholders, list);

                // Cache parsed format result, and format
                using (var formatParsed = new Parser().ParseFormat(indexPlaceholders))
                {
                    _ = smart.Format(formatParsed, list);
                    _ = smart.Format(formatParsed, list);
                }
                using (var formatParsed = new Parser().ParseFormat(listPlaceholders))
                {
                    _ = smart.Format(formatParsed, list);
                    _ = smart.Format(formatParsed, list);
                }
            }
        }

        [Test]
        public void No_Active_Pool_Items_After_Smart_Format()
        {
            var pools = GetAllSmartPoolsCleared();

            DoSomeFormattingWork();

            foreach (var p in pools)
            {
                Console.WriteLine(p.Type + @":");
                Console.WriteLine(@"{0}: {1}", nameof(IPoolCounters.CountActive), p.Counters?.CountActive);
                Console.WriteLine(@"{0}: {1}", nameof(IPoolCounters.CountInactive), p.Counters?.CountInactive);

                if (p.Counters!.CountAll <= 0) continue;
                
                Console.WriteLine();
                Assert.That(p.Counters.CountActive, Is.EqualTo(0), string.Join(" ", nameof(IPoolCounters.CountActive), p.Type?.ToString()));
                Assert.That(p.Counters.CountInactive, Is.GreaterThan(0), string.Join(" ", nameof(IPoolCounters.CountInactive), p.Type?.ToString()));
            }
        }

        [Test]
        public void Pooling_Disabled_Leaves_Pools_Empty()
        {
            var pools = GetAllSmartPoolsCleared();
            var savedPoolingEnabled = PoolSettings.IsPoolingEnabled; 
            PoolSettings.IsPoolingEnabled = false;

            DoSomeFormattingWork();

            PoolSettings.IsPoolingEnabled = savedPoolingEnabled;

            foreach (var p in pools)
            {
                Assert.That(p.Counters!.CountAll, Is.EqualTo(0), "CountAll");
            }
        }
    }
}
