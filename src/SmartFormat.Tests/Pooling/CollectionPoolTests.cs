using System.Collections.Generic;
using NUnit.Framework;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Tests.Pooling;

[TestFixture]
public class CollectionPoolTests
{
    private static CollectionPool<List<int>, int> GetCollectionPool()
    {
        CollectionPool<List<int>, int>.Instance.Clear();
        var cp = CollectionPool<List<int>, int>.Instance;
        return cp;
    }

    [Test]
    public void Create_New_Instance()
    {
        var cp = GetCollectionPool();
            
        Assert.That(() => cp.Get(), Throws.Nothing);
        Assert.Multiple(() =>
        {
            Assert.That(cp.Pool.CountActive, Is.EqualTo(1));
            Assert.That(cp.Pool.CountInactive, Is.EqualTo(0));
            Assert.That(cp.Pool.CountAll, Is.EqualTo(1));
        });
    }

    [Test]
    public void Get_Pooled_Instance()
    {
        var cp = GetCollectionPool();

        var list = cp.Get();

        Assert.That(cp.Pool.CountActive, Is.EqualTo(1));
        Assert.That(() => cp.Return(list), Throws.Nothing);
        Assert.Multiple(() =>
        {
            Assert.That(cp.Pool.CountActive, Is.EqualTo(0));
            Assert.That(cp.Pool.CountInactive, Is.EqualTo(1));
            Assert.That(cp.Pool.CountAll, Is.EqualTo(1));
        });
    }

    [Test]
    public void Get_PooledObject()
    {
        var cp = GetCollectionPool();
        var list = cp.Get();
        cp.Return(list);
        cp.Get(out var list2);
        Assert.That(list2, Is.SameAs(list));
    }
        
    [Test]
    public void Dispose_Pool()
    {
        var cp = GetCollectionPool();
        _ = cp.Get();
        cp.Dispose();

        Assert.That(cp.Pool.CountAll, Is.EqualTo(0));
    }
}
