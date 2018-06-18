using System.Xml.Linq;
using NUnit.Framework;
using SmartFormat.Utilities;

namespace SmartFormat.Tests.Extensions
{
    [TestFixture]
    public class PluralRulesTest
    {
        [Test]
        public void Illegal_Iso_Language()
        {
            Assert.IsNull(PluralRules.GetPluralRule("-for-sure-illegal-code"));
        }

        [Test]
        public void Get_All_Plural_Rules()
        {
            foreach (var deleg in PluralRules.IsoLangToDelegate)
            {
                var rule = PluralRules.GetPluralRule(deleg.Key);
                for (var i = 0; i <= 100; i++)
                {
                    // not testing plural rules logic
                    Assert.DoesNotThrow(() => rule(i, 1));
                }
            }
        }
    }
}
