using System.Threading.Tasks;
using NUnit.Framework;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
using SmartFormat.Extensions.PersistentVariables;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class GlobalVariableSourceTests
    {
        [Test]
        public void Global_Variables_In_Different_SmartFormatters()
        {
            const string formatString = "{global.theVariable}";

            var globalGrp = new VariablesGroup
                { { "theVariable", new StringVariable("val-from-global-source") } };

            GlobalVariablesSource.Instance.Add("global", globalGrp);

            var smart1 = new SmartFormatter();
            smart1.FormatterExtensions.Add(new DefaultFormatter());
            smart1.AddExtensions(0, GlobalVariablesSource.Instance);

            var smart2 = new SmartFormatter();
            smart2.FormatterExtensions.Add(new DefaultFormatter());
            smart2.AddExtensions(0, GlobalVariablesSource.Instance);

            var result1 = smart1.Format(formatString);
            var result2 = smart2.Format(formatString);

            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void Reset_Should_Create_A_New_Instance()
        {
            var globalGrp = new VariablesGroup
                { { "theVariable", new StringVariable("val-from-global-source") } };

            GlobalVariablesSource.Instance.Add("global", globalGrp);

            var ref1 = GlobalVariablesSource.Instance;
            GlobalVariablesSource.Reset();
            var ref2 = GlobalVariablesSource.Instance;
            
            Assert.That(!ReferenceEquals(ref1, ref2), "Different references after ResetInstance()");
            Assert.That(ref2.Count, Is.EqualTo(0));
        }

        [Test]
        public void Parallel_Load_By_Adding_Variables_To_Instance()
        {
            // Switch to thread safety - otherwise the test would throw an InvalidOperationException
            const bool currentThreadSafeMode = true;
            var savedIsThreadSafeMode = SmartSettings.IsThreadSafeMode;
            SmartSettings.IsThreadSafeMode = currentThreadSafeMode;

            GlobalVariablesSource.Instance.Add("global", new VariablesGroup());

            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Assert.That(code: () =>
                Parallel.For(0L, 1000, options, (i, loopState) =>
                {
                    GlobalVariablesSource.Instance["global"].Add($"{i:0000}", new IntVariable((int)i));
                }), Throws.Nothing);
            Assert.That(GlobalVariablesSource.Instance["global"].Count, Is.EqualTo(1000));

            // Restore to saved value
            SmartSettings.IsThreadSafeMode = savedIsThreadSafeMode;
        }
    }
}
