using NUnit.Framework;
using SmartFormat.Core.Output;
using SmartFormat.Pooling.SmartPools;

namespace SmartFormat.Tests.Pooling;

[TestFixture]
public class StringOutputPoolTests
{
    private static StringOutputPool GetStringOutputPool()
    {
        StringOutputPool.Instance.Clear();
        var sop = StringOutputPool.Instance;
        sop.Pool.IsPoolingEnabled = true;
        return sop;
    }
        
    [Test]
    public void Create_New_Instance()
    {
        var sop = GetStringOutputPool();
        StringOutput so = new();
        Assert.That(() => so = sop.Get(), Throws.Nothing);
        Assert.Multiple(() =>
        {
            Assert.That(sop.Pool.CountActive, Is.EqualTo(1));
            Assert.That(sop.Pool.CountInactive, Is.EqualTo(0));
            Assert.That(sop.Pool.CountAll, Is.EqualTo(1));

            Assert.That(() => sop.Return(so), Throws.Nothing);
        });
        Assert.Multiple(() =>
        {
            Assert.That(sop.Pool.CountActive, Is.EqualTo(0));
            Assert.That(sop.Pool.CountInactive, Is.EqualTo(1));
            Assert.That(sop.Pool.CountAll, Is.EqualTo(1));
        });
    }
}