using NUnit.Framework;

namespace SmartFormat.Tests.Core;

[TestFixture]
public class NestingTests
{
    public NestingTests()
    {
        _data = new {
            One = 1,
            ChildOne = new {
                Two = 2,
                ChildTwo = new {
                    Three = 3,
                    ChildThree = new {
                        Four = 4,
                    }
                }
            }
        };
    }
    private readonly object _data;

    [Test]
    [TestCase("{ChildOne.ChildTwo: {Three} {0.One} }", " 3 1 ")]
    [TestCase("{ChildOne.ChildTwo.ChildThree: {Four} {0.ChildOne: {Two} {0.One} } }", " 4  2 1  ")]
    public void Nesting_can_access_root_via_number(string format, string expectedOutput)
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var actual = smart.Format(format, _data);
        Assert.That(actual, Is.EqualTo(expectedOutput));
    }

    [Test]
    public void Nesting_CurrentScope_propertyName_Outrules_OuterScope_propertyName()
    {
        var smart = Smart.CreateDefaultSmartFormat();
        // Note the same property name in both parent and child scope
        var nestedObject = new {
            IdenticalName = "Name from parent",
            ParentValue = "Parent value",
            Child = new { IdenticalName = "Name from Child", ChildValue = "Child value" }
        };

        var nestedObject2 = new {
            IdenticalName = "Name from parent",
            ParentValue = "Parent value",
            // Property "IdenticalName" does not exist in child scope
            Child = new { ChildValue = "Child value" }
        };

        /*
            - If a property does not exist in the current (child) scope:
              SmartFormat will attempt to resolve the property in the outer (parent) scope. This is standard behavior and is demonstrated in your test `Nesting_CurrentScope_propertyName_outrules_OuterScope_propertyName()`.

            - If the property exists in the child scope but its value is NULL:  
              SmartFormat does NOT fallback to the parent scope. It treats the property as present,
              and the result is NULL.  
              It will only fallback if the property is missing (not defined) in the child scope.
         */

        Assert.Multiple(() =>
        {
            // Access to OUTER scope with scoped notation, if variable does NOT EXIST in current scope
            Assert.That(smart.Format("{Child:{ParentValue} - {ChildValue}}", nestedObject),
                Is.EqualTo(string.Format($"{nestedObject.ParentValue} - {nestedObject.Child.ChildValue}")));

            // Access to current scope, although outer scope variable with same name exists
            // This would also apply if the current scope variable was NULL
            Assert.That(smart.Format("{Child:{IdenticalName} - {IdenticalName}}", nestedObject),
                Is.Not.EqualTo(string.Format($"{nestedObject.IdenticalName} - {nestedObject.Child.IdenticalName}")));
            // but instead, even with mixed scoped and dot notation
            Assert.That(smart.Format("{Child:{IdenticalName} - {Child.IdenticalName}}", nestedObject),
                Is.EqualTo(string.Format($"{nestedObject.Child.IdenticalName} - {nestedObject.Child.IdenticalName}")));

            // Property does not exist in child scope, but exists in outer scope
            Assert.That(smart.Format("Fallback: {Child:{IdenticalName}}", nestedObject2), Is.EqualTo("Fallback: Name from parent"));
        });
    }

    [Test]
    [TestCase("{ChildOne.ChildTwo.ChildThree:{Four}{One}}","41")]
    [TestCase("{ChildOne:{ChildTwo:{ChildThree:{Four}{Three}{Two}{One}}}}","4321")]
    [TestCase("{ChildOne:{ChildTwo:{ChildThree:{Four}{ChildTwo.Three}{ChildOne.Two}{One}}}}","4321")]
    [TestCase("{ChildOne:{ChildTwo:{ChildThree:{ChildOne:{ChildTwo:{ChildThree:{Four}}}}}}}","4")]
    public void Nesting_can_access_outer_scopes_no_blanks(string format, string expectedOutput)
    {
        // Removing the spaces from Nesting_can_access_outer_scopes requires alternative escaping of { and }!
        var sf = Smart.CreateDefaultSmartFormat();
        var actual = sf.Format(format, _data);
        Assert.That(actual, Is.EqualTo(expectedOutput));
    }
}
