using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Pooling;
using SmartFormat.Pooling.ObjectPools;
using SmartFormat.Tests.TestUtils;

namespace SmartFormat.Tests.Pooling;

[TestFixture]
public class ObjectPoolClassesTests
{
    internal class SomePoolObject
    {
        public string Value { get; set; } = string.Empty;
    }

    private const int MaximumPoolSize = 5;

    // Used as TestCaseSource
    private static IEnumerable<ObjectPool<SomePoolObject>> GetObjectPoolBasedPools(bool withValidPolicy)
    {
        var policy = withValidPolicy
            ? new PoolPolicy<SomePoolObject>
            {
                FunctionOnCreate = () => new SomePoolObject { Value = "created" },
                ActionOnGet = o => o.Value = "get",
                ActionOnReturn = o => o.Value = "returned", 
                ActionOnDestroy = o => o.Value = "destroyed",
                MaximumPoolSize = MaximumPoolSize,
                InitialPoolSize = 1
            }
            : new PoolPolicy<SomePoolObject>(); // leaves 'FunctionOnCreate' unset

        var types = ReflectionTools.GetSubclassesOf(typeof(ObjectPool<>).Assembly, typeof(ObjectPool<>));
        foreach (var type in types)
        {
            var constructedType = type.MakeGenericType(typeof(SomePoolObject));
            var instance = (ObjectPool<SomePoolObject>)Activator.CreateInstance(constructedType, policy)!;
                
            instance.Clear();
            yield return instance;
        }
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { false })]
    public void Missing_Policy_For_FunctionOnCreate_Should_Throw(object poolAsObj)
    {
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;

        Assert.That(del: () => pool.Get(), Throws.InstanceOf<InvalidOperationException>(), poolAsObj.GetType().Name);
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Apply_Policy_For_Pool(object poolAsObj)
    {
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;

        var obj = pool.Get(); // first 'Get' ever creates a new instance
        var created = obj.Value.ToLower();
        pool.Return(obj);
        var returned = obj.Value.ToLower();
        obj = pool.Get();
        var get = obj.Value.ToLower();
        pool.Return(obj);
        pool.Clear(); // 'Clear' calls ActionOnDestroy
        var destroyed = obj.Value.ToLower();
        Assert.Multiple(() =>
        {
            Assert.That(created, Is.EqualTo(nameof(created)), poolAsObj.GetType().Name);
            Assert.That(returned, Is.EqualTo(nameof(returned)), poolAsObj.GetType().Name);
            Assert.That(get, Is.EqualTo(nameof(get)), poolAsObj.GetType().Name);
            Assert.That(destroyed, Is.EqualTo(nameof(destroyed)), poolAsObj.GetType().Name);
        });
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Apply_Policy_For_Pool_With_PooledObject(object poolAsObj)
    {
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;

        string? created;
        using (var po = pool.Get(out var obj)) // auto return
        {
            created = obj.Value.ToLower(); // first 'Get' ever creates a new instance
        }
            
        var returned = pool.PoolItems[0].Value;

        string? get;
        using (var po = pool.Get(out var obj)) // auto return
        {
            get = obj.Value.ToLower();
        }
            
        var itemInPool = pool.PoolItems[0]; // reference the first and only item
        pool.Clear(); // 'Clear' calls ActionOnDestroy
        var destroyed = itemInPool.Value;
        Assert.Multiple(() =>
        {
            Assert.That(created, Is.EqualTo(nameof(created)), poolAsObj.GetType().Name);
            Assert.That(returned, Is.EqualTo(nameof(returned)), poolAsObj.GetType().Name);
            Assert.That(get, Is.EqualTo(nameof(get)), poolAsObj.GetType().Name);
            Assert.That(destroyed, Is.EqualTo(nameof(destroyed)), poolAsObj.GetType().Name);
        });
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Return_Element_Twice_Should_Throw(object poolAsObj)
    {
        var checkObjectSetting = PoolSettings.CheckReturnedObjectsExistInPool;
        PoolSettings.CheckReturnedObjectsExistInPool = true; // throws with this setting
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;
        var obj = pool.Get();
        Assert.That(code: () => pool.Return(obj), Throws.Nothing, poolAsObj.GetType().Name);
        Assert.That(code: () => pool.Return(obj), Throws.TypeOf<PoolingException>(), poolAsObj.GetType().Name);
        PoolSettings.CheckReturnedObjectsExistInPool = checkObjectSetting;
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Return_Element_Twice_Should_Not_Throw(object poolAsObj)
    {
        var checkObjectSetting = PoolSettings.CheckReturnedObjectsExistInPool;
        PoolSettings.CheckReturnedObjectsExistInPool = false; // does not throw with this setting
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;
        var obj = pool.Get();
        Assert.That(code: () => pool.Return(obj), Throws.Nothing, poolAsObj.GetType().Name);
        Assert.That(code: () => pool.Return(obj), Throws.Nothing, poolAsObj.GetType().Name);
        PoolSettings.CheckReturnedObjectsExistInPool = checkObjectSetting;
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Check_Item_Counters(object poolAsObj)
    {
        const int numOfCreated = 3;

        var pool = (ObjectPool<SomePoolObject>) poolAsObj;
        var active = new List<SomePoolObject>();
        for (var i = 1; i <= numOfCreated; i++)
        {
            active.Add(pool.Get());
        }

        var activeCount1 = pool.CountActive;
        var allCount1 = pool.CountAll;
        var inactiveCount1 = pool.CountInactive;

        foreach (var a in active)
        {
            pool.Return(a);
        }

        var activeCount2 = pool.CountActive;
        var allCount2 = pool.CountAll;
        var inactiveCount2 = pool.CountInactive;

        pool.Clear();

        Assert.Multiple(() =>
        {
            // Got 3 elements from the pool
            Assert.That(activeCount1, Is.EqualTo(3), poolAsObj.GetType().Name);
            Assert.That(allCount1, Is.EqualTo(3), poolAsObj.GetType().Name);
            Assert.That(inactiveCount1, Is.EqualTo(0), poolAsObj.GetType().Name);

            // Returned 3 elements back to the pool
            Assert.That(activeCount2, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(allCount2, Is.EqualTo(3), poolAsObj.GetType().Name);
            Assert.That(inactiveCount2, Is.EqualTo(3), poolAsObj.GetType().Name);

            // After 'Clear'
            Assert.That(pool.CountAll, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountActive, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountInactive, Is.EqualTo(0), poolAsObj.GetType().Name);
        });
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Should_Not_Exceed_Maximum_Pool_Size(object poolAsObj)
    {
        const int numOfCreated = MaximumPoolSize + 5;

        var pool = (ObjectPool<SomePoolObject>) poolAsObj;
            
        var active = new List<SomePoolObject>();
        for (var i = 1; i <= numOfCreated; i++)
        {
            active.Add(pool.Get());
        }

        var activeCount1 = pool.CountActive;
        var allCount1 = pool.CountAll;
        var inactiveCount1 = pool.CountInactive;

        foreach (var a in active)
        {
            pool.Return(a);
        }

        var activeCount2 = pool.CountActive;
        var allCount2 = pool.CountAll;
        var inactiveCount2 = pool.CountInactive;

        pool.Clear();

        Assert.Multiple(() =>
        {
            // Got 'numOfCreated' elements from the pool
            Assert.That(activeCount1, Is.EqualTo(numOfCreated), poolAsObj.GetType().Name);
            Assert.That(allCount1, Is.EqualTo(numOfCreated), poolAsObj.GetType().Name);
            Assert.That(inactiveCount1, Is.EqualTo(0), poolAsObj.GetType().Name);

            // Tried to return all 'numOfCreated' elements back to the pool
            Assert.That(activeCount2, Is.EqualTo(MaximumPoolSize), poolAsObj.GetType().Name);
            Assert.That(allCount2, Is.EqualTo(numOfCreated), poolAsObj.GetType().Name);
            Assert.That(inactiveCount2, Is.EqualTo(MaximumPoolSize), poolAsObj.GetType().Name);

            // After 'Clear'
            Assert.That(pool.CountAll, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountActive, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountInactive, Is.EqualTo(0), poolAsObj.GetType().Name);
        });
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Dispose_Should_Clear_The_Pool(object poolAsObj)
    {
        const int shouldBeCreated = 5;
        
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;
            
        for (var i = 1; i <= shouldBeCreated; i++)
        {
            _ = pool.Get();
        }

        var actualCreated = pool.CountAll;

        pool.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(actualCreated, Is.EqualTo(shouldBeCreated), poolAsObj.GetType().Name);
            Assert.That(pool.CountAll, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountActive, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountInactive, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.PoolItems.Any(), Is.EqualTo(false), poolAsObj.GetType().Name);
        });
    }

    [TestCaseSource(nameof(GetObjectPoolBasedPools), new object?[] { true })]
    public void Disabled_Pooling_Should_Only_Return_New_Instances(object poolAsObj)
    {
        var pool = (ObjectPool<SomePoolObject>) poolAsObj;
        var savedPoolingEnabled = PoolSettings.IsPoolingEnabled;
        PoolSettings.IsPoolingEnabled = false;
            
        var active = new List<SomePoolObject>();
        for (var i = 1; i <= 5; i++)
        {
            active.Add(pool.Get());
        }

        foreach (var a in active)
        {
            pool.Return(a);
        }
        PoolSettings.IsPoolingEnabled = savedPoolingEnabled;

        Assert.Multiple(() =>
        {
            Assert.That(pool.CountAll, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountActive, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.CountInactive, Is.EqualTo(0), poolAsObj.GetType().Name);
            Assert.That(pool.PoolItems.Any(), Is.EqualTo(false), poolAsObj.GetType().Name);
        });
    }
}
