using NUnit.Framework;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Tests.Pooling;

[TestFixture]
public class ListPoolTests
{
    private static ListPool<string> GetListPool()
    {
        ListPool<string>.Instance.Clear();
        var lp = ListPool<string>.Instance;
        lp.Pool.IsPoolingEnabled = true;
        return lp;
    }
        
    [Test]
    public void Create_New_Instance()
    {
        var lp = GetListPool();
            
        Assert.That(() => lp.Get(), Throws.Nothing);
        Assert.That(lp.Pool.CountActive, Is.EqualTo(1));
        Assert.That(lp.Pool.CountInactive, Is.EqualTo(0));
        Assert.That(lp.Pool.CountAll, Is.EqualTo(1));
    }
}