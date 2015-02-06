using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using ExpectedResults = System.Collections.Generic.Dictionary<decimal, string>;

namespace SmartFormat.Tests
{
	[TestFixture]
	public class PluralLocalizationFormatterTests
	{
		private void TestAllResults(CultureInfo cultureInfo, string format, ExpectedResults expectedValuesAndResults)
		{
			foreach (var test in expectedValuesAndResults)
			{
				var value = test.Key;
				var expected = test.Value;
				var actual = Smart.Format(cultureInfo, format, value);

				Assert.That(actual, Is.EqualTo(expected));
				Debug.WriteLine(actual);
			}
		}

		[Test]
		public void Test_Default()
		{
			TestAllResults(
				CultureInfo.CreateSpecificCulture("en-us"),
				"There {0:is|are} {0} {0:item|items} remaining",
				new ExpectedResults {
					{  -1, "There are -1 items remaining"},
					{   0, "There are 0 items remaining"},
					{0.5m, "There are 0.5 items remaining"},
					{   1, "There is 1 item remaining"},
					{1.5m, "There are 1.5 items remaining"},
					{   2, "There are 2 items remaining"},
					{  11, "There are 11 items remaining"},
				});
		}

		[Test]
		public void Test_English()
		{
			TestAllResults(
				CultureInfo.GetCultureInfo("en-US"),
				"There {0:is|are} {0} {0:item|items} remaining",
				new ExpectedResults {
					{  -1, "There are -1 items remaining"},
					{   0, "There are 0 items remaining"},
					{0.5m, "There are 0.5 items remaining"},
					{   1, "There is 1 item remaining"},
					{1.5m, "There are 1.5 items remaining"},
					{   2, "There are 2 items remaining"},
					{  11, "There are 11 items remaining"},
				});
		}

		[Test]
		public void Test_Turkish()
		{
			TestAllResults(
				CultureInfo.GetCultureInfo("tr-TR"),
				"{0} nesne kaldı.",
				new ExpectedResults {
					{   0, "0 nesne kaldı."},
					{   1, "1 nesne kaldı."},
					{   2, "2 nesne kaldı."},
				});

			TestAllResults(
				CultureInfo.GetCultureInfo("tr"),
				"Seçili {0:nesneyi|nesneleri} silmek istiyor musunuz?",
				new ExpectedResults {
					{  -1, "Seçili nesneleri silmek istiyor musunuz?"},
					{   0, "Seçili nesneleri silmek istiyor musunuz?"},
					{0.5m, "Seçili nesneleri silmek istiyor musunuz?"},
					{   1, "Seçili nesneyi silmek istiyor musunuz?"},
					{1.5m, "Seçili nesneleri silmek istiyor musunuz?"},
					{   2, "Seçili nesneleri silmek istiyor musunuz?"},
					{  11, "Seçili nesneleri silmek istiyor musunuz?"},
				});
		}

		[Test]
		public void Test_Russian()
		{
			TestAllResults(
				CultureInfo.GetCultureInfo("ru-RU"),
				"Я купил {0} {0:банан|банана|бананов}.",
				new ExpectedResults {
					{   0, "Я купил 0 бананов."},
					{   1, "Я купил 1 банан."},
					{   2, "Я купил 2 банана."},
					{  11, "Я купил 11 бананов."},
					{  20, "Я купил 20 бананов."},
					{  21, "Я купил 21 банан."},
					{  22, "Я купил 22 банана."},
					{  25, "Я купил 25 бананов."},
					{  120, "Я купил 120 бананов."},
					{  121, "Я купил 121 банан."},
					{  122, "Я купил 122 банана."},
					{  125, "Я купил 125 бананов."},
				});
		}

		[Test]
		public void Test_Polish()
		{
			TestAllResults(
				CultureInfo.GetCultureInfo("pl"),
				"{0} {0:miesiąc|miesiące|miesięcy} temu",
				new ExpectedResults {
					{   0, "0 miesięcy temu"},
					{   1, "1 miesiąc temu"},
					{   2, "2 miesiące temu"},
					{   3, "3 miesiące temu"},
					{   4, "4 miesiące temu"},
					{   5, "5 miesięcy temu"},
					{   6, "6 miesięcy temu"},
					{   7, "7 miesięcy temu"},
					{   8, "8 miesięcy temu"},
					{   9, "9 miesięcy temu"},
					{  10, "10 miesięcy temu"},
					{  11, "11 miesięcy temu"},
					{  12, "12 miesięcy temu"},
					{  13, "13 miesięcy temu"},
					{  14, "14 miesięcy temu"},
					{  15, "15 miesięcy temu"},
					{  16, "16 miesięcy temu"},
					{  17, "17 miesięcy temu"},
					{  18, "18 miesięcy temu"},
					{  19, "19 miesięcy temu"},
					{  20, "20 miesięcy temu"},
					{  21, "21 miesięcy temu"},
					{  22, "22 miesiące temu"},
					{  23, "23 miesiące temu"},
					{  24, "24 miesiące temu"},
					{  25, "25 miesięcy temu"},
					{  100, "100 miesięcy temu"},
					{  101, "101 miesięcy temu"},
					{  102, "102 miesiące temu"},
					{  103, "103 miesiące temu"},
					{  104, "104 miesiące temu"},
					{  105, "105 miesięcy temu"},
				});
		}


	}
}
