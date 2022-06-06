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
        Assert.AreEqual(expectedOutput, actual);
    }

    [Test]
    public void Nesting_CurrentScope_propertyName_outrules_OuterScope_propertyName()
    {
        var smart = Smart.CreateDefaultSmartFormat();
        var nestedObject = new
        {
            IdenticalName = "Name from parent", 
            ParentValue = "Parent value",
            Child = new {IdenticalName = "Name from Child", ChildValue = "Child value"}
        };

        // Access to outer scope, if no current scope variable is found
        Assert.AreEqual(string.Format($"{nestedObject.ParentValue} - {nestedObject.Child.ChildValue}"), smart.Format("{Child:{ParentValue} - {Child.ChildValue}|}", nestedObject));

        // Access to current scope, although outer scope variable with same name exists
        Assert.AreNotEqual(string.Format($"{nestedObject.IdenticalName} - {nestedObject.Child.IdenticalName}"), smart.Format("{Child:{IdenticalName} - {Child.IdenticalName}|}", nestedObject));
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
        Assert.AreEqual(expectedOutput, actual);
    }
}