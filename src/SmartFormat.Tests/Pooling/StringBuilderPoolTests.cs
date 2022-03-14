using System.Text;
using NUnit.Framework;
using SmartFormat.Pooling;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Tests.Pooling
{
    [TestFixture]
    public class StringBuilderPoolTests
    {
        private static StringBuilderPool GetStringBuilderPool()
        {
            StringBuilderPool.Instance.Clear();
            var sbp = StringBuilderPool.Instance;
            sbp.Pool.IsPoolingEnabled = true;
            return sbp;
        }

        [Test]
        public void Create_New_Instance()
        {
            var sbp = GetStringBuilderPool();
            
            Assert.That(() => sbp.Get(), Throws.Nothing);
            Assert.That(sbp.Pool.CountActive, Is.EqualTo(1));
            Assert.That(sbp.Pool.CountInactive, Is.EqualTo(0));
            Assert.That(sbp.Pool.CountAll, Is.EqualTo(1));
        }

        [Test]
        public void Get_Pooled_Instance()
        {
            var sbp = GetStringBuilderPool();
            sbp.DefaultStringBuilderCapacity = 1234;

            var sb = sbp.Get();
            sb.Append("something");

            Assert.That(sbp.Pool.CountActive, Is.EqualTo(1));
            Assert.That(sb.Capacity, Is.EqualTo(sbp.DefaultStringBuilderCapacity));
            
            // Returning an item should clear the StringBuilder
            Assert.That(() => sbp.Return(sb), Throws.Nothing);
            Assert.That(sb.Length, Is.EqualTo(0));
            
            Assert.That(sbp.Pool.CountActive, Is.EqualTo(0));
            Assert.That(sbp.Pool.CountInactive, Is.EqualTo(1));
            Assert.That(sbp.Pool.CountAll, Is.EqualTo(1));
        }

        [Test]
        public void Get_PooledObject()
        {
            var sbp = GetStringBuilderPool();
            var sb = sbp.Get();
            sbp.Return(sb);
            sbp.Get(out var sb2);
            Assert.AreSame(sb, sb2);
        }

        [Test]
        public void Reset_Pool()
        {
            var sbp = GetStringBuilderPool();
            
            var savedIsThreadSafeMode = sbp.Pool.IsThreadSafeMode;
            var savedObjectPoolType = sbp.Pool.GetType();
            
            // Change the current setting
            sbp.Reset(!savedIsThreadSafeMode);
            
            var newThreadSafety = !savedIsThreadSafeMode;
            var newObjectPoolType = sbp.Pool.GetType();

            Assert.That(savedObjectPoolType,
                savedIsThreadSafeMode
                    ? Is.EqualTo(typeof(ObjectPoolConcurrent<StringBuilder>))
                    : Is.EqualTo(typeof(ObjectPoolSingleThread<StringBuilder>)));
            
            Assert.That(newObjectPoolType,
                newThreadSafety
                    ? Is.EqualTo(typeof(ObjectPoolConcurrent<StringBuilder>))
                    : Is.EqualTo(typeof(ObjectPoolSingleThread<StringBuilder>)));
            Assert.That(newThreadSafety, Is.EqualTo(sbp.Pool.IsThreadSafeMode));

            // Clean up
            sbp.Reset(savedIsThreadSafeMode);
        }
        
        [Test]
        public void Dispose_Pool()
        {
            _ = StringBuilderPool.Instance; // ensure it is in the registry
            var sbp = PoolRegistry.Get<StringBuilderPool>();
            var stringBuilder = sbp?.Get();
            sbp?.Dispose();

            Assert.That(stringBuilder, Is.Not.Null, "StringBuilder instance");
            Assert.That(sbp?.Pool.CountAll ?? -1, Is.EqualTo(0), "CountAll");
        }
    }
}
