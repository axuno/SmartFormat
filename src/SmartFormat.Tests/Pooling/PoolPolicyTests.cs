using NUnit.Framework;
using SmartFormat.Pooling;
using SmartFormat.Pooling.ObjectPools;

namespace SmartFormat.Tests.Pooling
{
    [TestFixture]
    public class PoolPolicyTests
    {
        [Test]
        public void Illegal_Pool_Size_Should_Throw()
        {
            Assert.That(() => new PoolPolicy<object> { MaximumPoolSize = 0 },
                Throws.InstanceOf(typeof(PoolingException))
                    .And
                    .Property(nameof(PoolingException.PoolType))
                    .EqualTo(typeof(object)));
        }
    }
}
