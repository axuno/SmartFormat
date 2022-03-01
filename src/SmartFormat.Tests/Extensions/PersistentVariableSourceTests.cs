using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Extensions.PersistentVariables;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class PersistentVariableSourceTests
    {
        [Test]
        public void Is_Not_ReadOnly()
        {
            Assert.That(new PersistentVariablesSource().IsReadOnly, Is.False);
        }

        [Test]
        public void Add_And_Get_Items()
        {
            const string groupName1 = "global";
            const string groupName2 = "secret";

            var pvs = new PersistentVariablesSource();
            var vg1 = new VariablesGroup();
            var vg2 = new VariablesGroup();

            pvs.Add(groupName1, vg1);
            pvs[groupName2] = vg2;

            Assert.That(pvs.Count, Is.EqualTo(2));
            Assert.That(pvs[groupName1], Is.EqualTo(vg1));
            Assert.That(pvs[groupName2], Is.EqualTo(vg2));
            Assert.That(pvs.Keys.Count, Is.EqualTo(2));
            Assert.That(pvs.ContainsKey(groupName1));
            Assert.That(pvs.Values.Count, Is.EqualTo(2));
            Assert.That(pvs.Values.Contains(vg1));
            Assert.That(pvs.Values.Contains(vg2));
            Assert.That(pvs.ContainsKey(groupName2));
            Assert.That(pvs.TryGetValue(groupName1, out _), Is.True);
            Assert.That(pvs.TryGetValue(groupName1 + "False", out _), Is.False);
        }

        [Test]
        public void Add_Key_Value_Pair()
        {
            var pvs = new PersistentVariablesSource();
            var kvp = new KeyValuePair<string, VariablesGroup>("theKey", new VariablesGroup());
            pvs.Add(kvp);

            Assert.That(pvs.Contains(new KeyValuePair<string, VariablesGroup>(kvp.Key, kvp.Value)));
        }

        [Test]
        public void KeyValuePairs_By_Enumerators()
        {
            var pvs = new PersistentVariablesSource();
            var kvp = new KeyValuePair<string, VariablesGroup>("theKey", new VariablesGroup());
            pvs.Add(kvp);
            var kvpFromEnumerator = pvs.FirstOrDefault(keyValuePair => keyValuePair.Key.Equals("theKey"));

            // Test GetEnumerator()
            foreach (var keyValuePair in pvs)
            {
                Assert.That(keyValuePair, Is.EqualTo(kvp));
            }

            // Test IEnumerator<KeyValuePair<string, VariablesGroup>>
            Assert.That(kvpFromEnumerator, Is.EqualTo(kvp));
        }

        [Test]
        public void Add_Item_With_Illegal_Name_Should_Throw()
        {
            var pvs = new PersistentVariablesSource();
            var vg = new VariablesGroup();

            // Name must not be empty
            Assert.That(code: () => vg.Add(string.Empty, new IntVariable(1)), Throws.ArgumentException);
            Assert.That(code: () => pvs.Add(string.Empty, vg), Throws.ArgumentException);
        }

        [Test]
        public void Remove_Item()
        {
            var pvs = new PersistentVariablesSource();
            var kvp = new KeyValuePair<string, VariablesGroup>("theKey", new VariablesGroup());
            pvs.Add(kvp);
            pvs.Remove(kvp);

            Assert.That(pvs.Count, Is.EqualTo(0));
            Assert.That(pvs.Remove("non-existent"), Is.EqualTo(false));
        }

        [Test]
        public void Remove_Key_Value_Pair()
        {
            var pvs = new PersistentVariablesSource { { "global", new VariablesGroup() } };
            pvs.Remove("global");

            Assert.That(pvs.Count, Is.EqualTo(0));
            Assert.That(pvs.Remove("non-existent"), Is.EqualTo(false));
        }

        [Test]
        public void Clear_All_Items()
        {
            var pvs = new PersistentVariablesSource();
            var kvp = new KeyValuePair<string, VariablesGroup>("theKey", new VariablesGroup());
            pvs.Add(kvp);
            pvs.Clear();

            Assert.That(pvs.Count, Is.EqualTo(0));
        }

        [Test]
        public void Copy_To_Array()
        {
            var pvs = new PersistentVariablesSource();
            var kvp1 = new KeyValuePair<string, VariablesGroup>("key1", new VariablesGroup());
            var kvp2 = new KeyValuePair<string, VariablesGroup>("key2", new VariablesGroup());
            pvs.Add(kvp1);
            pvs.Add(kvp2);

            var array = new KeyValuePair<string, VariablesGroup>[pvs.Count];
            pvs.CopyTo(array, 0);

            Assert.That(pvs.Count, Is.EqualTo(array.Length));
            for (var i = 0; i < array.Length; i++)
            {
                Assert.That(pvs.ContainsKey(array[i].Key));
            }
        }

        [Test]
        public void Shallow_Copy()
        {
            var pvs = new PersistentVariablesSource();

            var kvp1 = new KeyValuePair<string, IVariable>("theKey1", new ObjectVariable("123"));
            var kvp2 = new KeyValuePair<string, IVariable>("theKey2", new FloatVariable(987.654f));

            var vg1 = new VariablesGroup { kvp1 };
            var vg2 = new VariablesGroup { kvp2 };

            pvs.Add(vg1.First().Key, vg1);
            pvs.Add(vg2.First().Key, vg2);

            var pvsCopy = pvs.Clone();

            Assert.That(pvsCopy.Count, Is.EqualTo(pvs.Count));
            Assert.That(pvsCopy.Values, Is.EquivalentTo(pvs.Values));
            Assert.That(pvsCopy.Keys, Is.EquivalentTo(pvs.Keys));
        }

        [Test]
        public void Use_Globals_Without_Args_To_Formatter()
        {
            // The top container
            // It gets its name later, when being added to the PersistentVariablesSource
            var varGroup = new VariablesGroup();
            
            // Add a (nested) VariablesGroup named 'group' to the top container
            varGroup.Add("group", new VariablesGroup
            {
                // Add variables to the nested group
                { "groupString", new StringVariable("groupStringValue") },
                { "groupDateTime", new Variable<DateTime>(new DateTime(2024, 12, 31)) }
            });
            // Add more variables to the top group container
            varGroup.Add(new KeyValuePair<string, IVariable>("topInteger", new IntVariable(12345)));
            var stringVar = new StringVariable("topStringValue");
            varGroup.Add("topString", stringVar);

            // The formatter for persistent variables requires only 2 extensions
            var smart = new SmartFormatter();
            smart.FormatterExtensions.Add(new DefaultFormatter());
            // Add as the first in the source extensions list
            var pvs = new PersistentVariablesSource
            {
                // Here, the top container gets its name
                { "global", varGroup }
            };
            smart.InsertExtension(0, pvs);

            // Act
            // Note: We don't need args to the formatter for globals
            var globalGroup = smart.Format(CultureInfo.InvariantCulture,
                "{global.group.groupString} {global.group.groupDateTime:'groupDateTime='yyyy-MM-dd}");
            var topInteger = smart.Format("{global.topInteger}");
            var topString = smart.Format("{global.topString}");

            // Assert
            Assert.That(globalGroup, Is.EqualTo("groupStringValue groupDateTime=2024-12-31"));
            Assert.That(topString, Is.EqualTo(stringVar.ToString()));
            Assert.That(topInteger, Is.EqualTo("12345"));
        }

        [Test]
        public void NonExisting_GroupVariable_Should_Throw()
        {
            var varGrp = new VariablesGroup { { "existing", new StringVariable("existing-value") } };

            var smart = new SmartFormatter();
            smart.FormatterExtensions.Add(new DefaultFormatter());
            smart.InsertExtension(0, new PersistentVariablesSource());
            var resultExisting = smart.Format("{existing}", varGrp);

            Assert.That(resultExisting, Is.EqualTo("existing-value"));
            Assert.That(code: () => smart.Format("{non-existing}", varGrp),
                Throws.InstanceOf<FormattingException>().And.Message.Contains("non-existing"));
        }

        [Test]
        public void PersistentVariablesSource_NameGroupPair()
        {
            var pvs = new PersistentVariablesSource.NameGroupPair("theName",
                new VariablesGroup
                    { new KeyValuePair<string, IVariable>("varName", new IntVariable(123)) });

            Assert.That(pvs.Name, Is.EqualTo("theName"));
            Assert.That(pvs.Group.ContainsKey("varName"), Is.True);
        }

        [Test]
        public void Format_Args_Should_Override_Persistent_Vars()
        {
            const string formatString = "{global.theVariable}";

            // Setup PersistentVariablesSource

            var persistentGrp = new VariablesGroup
                { { "theVariable", new StringVariable("val-from-persistent-source") } };

            var smart = new SmartFormatter();
            smart.FormatterExtensions.Add(new DefaultFormatter());
            smart.InsertExtension(0, new PersistentVariablesSource {{"global", persistentGrp}});

            // Setup equivalent VariablesGroup to use as an argument to Smart.Format(...)

            var argumentGrp = new VariablesGroup
                { { "global", new VariablesGroup { { "theVariable", new StringVariable("val-from-argument") } } } };

            // Arguments override PersistentVariablesSource
            var argumentResult = smart.Format(formatString, argumentGrp);
            // Without arguments, the variable from PersistentVariablesSource is used
            var persistentResult = smart.Format(formatString);

            Assert.That(argumentResult, Is.EqualTo("val-from-argument"));
            Assert.That(persistentResult, Is.EqualTo("val-from-persistent-source"));
        }

        [Test]
        public void Parallel_Load_By_Adding_Variables_To_Source()
        {
            // Switch to thread safety - otherwise the test would throw an InvalidOperationException
            const bool currentThreadSafeMode = true;
            var savedIsThreadSafeMode = SmartSettings.IsThreadSafeMode;
            SmartSettings.IsThreadSafeMode = currentThreadSafeMode;

            var pvs = new PersistentVariablesSource { { "global", new VariablesGroup() } };
            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Assert.That(code: () =>
                Parallel.For(0L, 1000, options, (i, loopState) =>
                {
                    pvs["global"].Add($"{i:0000}", new IntVariable((int)i));
                }), Throws.Nothing);
            Assert.That(pvs["global"].Count, Is.EqualTo(1000));

            // Restore to saved value
            SmartSettings.IsThreadSafeMode = savedIsThreadSafeMode;
        }
    }
}
