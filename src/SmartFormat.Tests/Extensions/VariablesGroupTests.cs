using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Extensions;
using SmartFormat.Extensions.PersistentVariables;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class VariablesGroupTests
    {
        [Test]
        public void Is_Not_ReadOnly()
        {
            Assert.That(new VariablesGroup().IsReadOnly, Is.False);
        }

        [Test]
        public void Add_And_Get_Items()
        {
            const string var1Name = "var1name";
            const string var2Name = "var2name";
            const string var3Name = "var3name";

            var var1 = new IntVariable(1234);
            var var2 = new StringVariable("theValue");
            var var3 = new BoolVariable(true);

            var vg = new VariablesGroup
            {
                { var1Name, var1 },
                new KeyValuePair<string, IVariable>(var2Name, var2)
            };
            vg[var3Name] = var3;
            
            Assert.That(vg.Count, Is.EqualTo(3));
            Assert.That((int) vg[var1Name].GetValue()!, Is.EqualTo(1234));
            Assert.That((string) vg[var2Name].GetValue()!, Is.EqualTo("theValue"));
            Assert.That(vg.Keys.Count, Is.EqualTo(3));
            Assert.That(vg.Keys.Contains(var1Name));
            Assert.That(vg.Values.Count, Is.EqualTo(3));
            Assert.That(vg.Values.Contains(var1));
            Assert.That(vg.Values.Contains(var2));
            Assert.That(vg.Values.Contains(var3));
            Assert.That(vg.ContainsKey(var2Name));
            Assert.That(vg.TryGetValue(var1Name + "False", out _), Is.False);
        }

        [Test]
        public void Add_Key_Value_Pair()
        {
            var vg = new VariablesGroup();
            var kvp = new KeyValuePair<string, IVariable>("theKey", new IntVariable(9876));
            vg.Add(kvp);

            Assert.That(vg.Contains(new KeyValuePair<string, IVariable>(kvp.Key, kvp.Value)));
        }

        [Test]
        public void KeyValuePairs_By_Enumerators()
        {
            var vg = new VariablesGroup();
            var kvp = new KeyValuePair<string, IVariable>("theKey", new IntVariable(9876));
            vg.Add(kvp);
            var kvpFromEnumerator = vg.FirstOrDefault(keyValuePair => keyValuePair.Key.Equals("theKey"));

            // Test GetEnumerator()
            foreach (var keyValuePair in vg)
            {
                Assert.That(keyValuePair, Is.EqualTo(kvp));
            }

            // Test IEnumerator<KeyValuePair<string, VariablesGroup>>
            Assert.That(kvpFromEnumerator, Is.EqualTo(kvp));
        }

        [Test]
        public void Add_Item_With_Illegal_Name_Should_Throw()
        {
            var vg = new VariablesGroup();

            // Name must not be empty
            Assert.That(code: () => vg.Add(string.Empty, new IntVariable(1)), Throws.ArgumentException);
        }

        [Test]
        public void Remove_Item()
        {
            var vg = new VariablesGroup();
            var kvp = new KeyValuePair<string, IVariable>("theKey", new IntVariable(12));
            vg.Add(kvp);
            vg.Remove(kvp);

            Assert.That(vg.Count, Is.EqualTo(0));
            Assert.That(vg.Remove("non-existent"), Is.EqualTo(false));
        }

        [Test]
        public void Remove_Key_Value_Pair()
        {
            var vg = new VariablesGroup { { "theKey", new IntVariable(12) } };
            vg.Remove("theKey");

            Assert.That(vg.Count, Is.EqualTo(0));
            Assert.That(vg.Remove("non-existent"), Is.EqualTo(false));
        }

        [Test]
        public void Clear_All_Items()
        {
            var vg = new VariablesGroup();
            var kvp = new KeyValuePair<string, IVariable>("theKey", new IntVariable(135));
            vg.Add(kvp);
            vg.Clear();

            Assert.That(vg.Count, Is.EqualTo(0));
        }

        [Test]
        public void Copy_To_Array()
        {
            var vg = new VariablesGroup();
            var kvp1 = new KeyValuePair<string, IVariable>("theKey1", new IntVariable(135));
            var kvp2 = new KeyValuePair<string, IVariable>("theKey2", new IntVariable(987));
            vg.Add(kvp1);
            vg.Add(kvp2);

            var array = new KeyValuePair<string, IVariable>[vg.Count];
            vg.CopyTo(array, 0);

            Assert.That(vg.Count, Is.EqualTo(array.Length));
            for (var i = 0; i < array.Length; i++)
            {
                Assert.That(vg.ContainsKey(array[i].Key));
            }
        }

        [Test]
        public void Shallow_Copy()
        {
            var vg = new VariablesGroup();
            var kvp1 = new KeyValuePair<string, IVariable>("theKey1", new ObjectVariable("123"));
            var kvp2 = new KeyValuePair<string, IVariable>("theKey2", new FloatVariable(987.654f));
            vg.Add(kvp1);
            vg.Add(kvp2);
            var vgCopy = vg.Clone();

            Assert.That(vgCopy.Count, Is.EqualTo(vg.Count));
            Assert.That(vgCopy.Values, Is.EquivalentTo(vg.Values));
            Assert.That(vgCopy.Keys, Is.EquivalentTo(vg.Keys));
        }

        [Test]
        public void NameVariablePair_Test()
        {
            var nv = new NameVariablePair("theName", new IntVariable(1234));

            Assert.That(nv.Name, Is.EqualTo("theName"));
            Assert.That((int) nv.Variable.GetValue()!, Is.EqualTo(1234));
            Assert.That(nv.ToString(), Is.EqualTo("'theName' - 'System.Int32' - '1234'"));
        }
    }
}
