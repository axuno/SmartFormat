using NUnit.Framework;
using SmartFormat.Pooling.SpecializedPools;

namespace SmartFormat.Tests.Pooling;

[TestFixture]
public class DictionaryPoolTests
{
    private static DictionaryPool<int, string> GetDictionaryPool()
    {
        DictionaryPool<int, string>.Instance.Clear();
        var dp = DictionaryPool<int, string>.Instance;
        return dp;
    }

    [Test]
    public void Create_New_Instance()
    {
        var dictPool = GetDictionaryPool();
            
        Assert.That(() => dictPool.Get(), Throws.Nothing);
        Assert.Multiple(() =>
        {
            Assert.That(dictPool.Pool.CountActive, Is.EqualTo(1));
            Assert.That(dictPool.Pool.CountInactive, Is.EqualTo(0));
            Assert.That(dictPool.Pool.CountAll, Is.EqualTo(1));
        });
    }
}
