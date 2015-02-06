using SmartFormat.Extensions;

namespace SmartFormat.Utilities
{
	public static class PluralRules_Old
	{
		// Much of this language information came from the following url:
		// http://www.gnu.org/s/hello/manual/gettext/Plural-forms.html
		// The following language codes came from:
		// http://www.loc.gov/standards/iso639-2/php/code_list.php

		public static PluralRules.PluralRuleDelegate GetPluralRule(string twoLetterISOLanguageName)
		{
			switch (twoLetterISOLanguageName)
			{
				// Germanic family
				//	 English, German, Dutch, Swedish, Danish, Norwegian, Faroese
				// Romanic family
				//	 Spanish, Portuguese, Italian, Bulgarian
				// Latin/Greek family
				//	 Greek
				// Finno-Ugric family
				//	 Finnish, Estonian
				// Semitic family
				//	 Hebrew
				// Artificial
				//	 Esperanto
				// Finno-Ugric family
				//	 Hungarian
				// Turkic/Altaic family
				//	 Turkish
				case "en":
				case "de":
				case "nl":
				case "sv":
				case "da":
				case "no":
				case "nn":
				case "nb":
				case "fo":
				case "es":
				case "pt":
				case "it":
				case "bg":
				case "el":
				case "fi":
				case "et":
				case "he":
				case "eo":
				case "hu":
				case "tr":
					return English_Special;

				// Romanic family
				//	 Brazilian Portuguese, French
				case "fr":
					return French;

				// Baltic family
				//	 Latvian
				case "lv":
					return Latvian;

				// Celtic
				//	 Gaeilge (Irish)
				case "ga":
					return Irish;

				// Romanic family
				//	 Romanian
				case "ro":
					return Romanian;

				//Baltic family
				//	Lithuanian
				case "lt":
					return Lithuanian;

				// Slavic family
				//	 Russian, Ukrainian, Serbian, Croatian
				case "ru":
				case "uk":
				case "sr":
				case "hr":
					return Russian;

				// Slavic family
				//	 Czech, Slovak
				case "cs":
				case "sk":
					return Czech;

				// Slavic family
				//	 Polish
				case "pl":
					return Polish;

				// Slavic family
				//	 Slovenian
				case "sl":
					return Slovenian;


				default:
					return null;
			}
		}

		public static int English_Special(decimal value, int pluralCount)
		{
			// Two forms, singular used for one only
			if (pluralCount == 2)
			{
				return (value == 1m ? 0 : 1);
			}
			// Three forms (zero, one, plural)
			if (pluralCount == 3)
			{
				return (value == 0m) ? 0 : (value == 1m) ? 1 : 2;
			}
			// Four forms (negative, zero, one, plural)
			if (pluralCount == 4)
			{
				return (value < 0m) ? 0 : (value == 0m) ? 1 : (value == 1m) ? 2 : 3;
			}

			return -1; // Too many parameters!
		}
		public static int French(decimal value, int pluralCount)
		{
			// Two forms, singular used for zero and one
			if (pluralCount == 2)
			{
				return (value == 0m || value == 1m) ? 0 : 1;
			}
			return -1;
		}
		public static int Latvian(decimal value, int pluralCount)
		{
			// Three forms, special case for zero
			if (pluralCount == 3)
			{
				return (value % 10 == 1 && value % 100 != 11) ? 0 : (value != 0) ? 1 : 2;
			}
			return -1;
		}
		public static int Irish(decimal value, int pluralCount)
		{
			// Three forms, special cases for one and two
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value == 2) ? 1 : 2;

			}
			return -1;
		}
		public static int Romanian(decimal value, int pluralCount)
		{
			// Three forms, special case for numbers ending in 00 or [2-9][0-9]
			if (pluralCount == 3)
			{
				return (value == 1m) ? 0 : (value == 0m || (value % 100 > 0 && value % 100 < 20)) ? 1 : 2;
			}
			return -1;
		}
		public static int Lithuanian(decimal value, int pluralCount)
		{
			// Three forms, special case for numbers ending in 1[2-9]
			if (pluralCount == 3)
			{
				return (value % 10 == 1 && value % 100 != 11) ? 0 : (value % 10 >= 2 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
			}
			return -1;
		}
		public static int Russian(decimal value, int pluralCount)
		{
			// Three forms, special cases for numbers ending in 1 and 2, 3, 4, except those ending in 1[1-4]
			if (pluralCount == 3)
			{
				return (value % 10 == 1 && value % 100 != 11) ? 0 : (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
			}
			return -1;
		}
		public static int Czech(decimal value, int pluralCount)
		{
			// Three forms, special cases for 1 and 2, 3, 4
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value >= 2 && value <= 4) ? 1 : 2;
			}
			return -1;
		}
		public static int Polish(decimal value, int pluralCount)
		{
			// Three forms, special case for one and some numbers ending in 2, 3, or 4
			if (pluralCount == 3)
			{
				return (value == 1) ? 0 : (value % 10 >= 2 && value % 10 <= 4 && (value % 100 < 10 || value % 100 >= 20)) ? 1 : 2;
			}
			return -1;
		}
		public static int Slovenian(decimal value, int pluralCount)
		{
			// Four forms, special case for one and all numbers ending in 02, 03, or 04
			if (pluralCount == 4)
			{
				return (value % 100 == 1) ? 0 : (value % 100 == 2) ? 1 : (value % 100 == 3 || value % 100 == 4) ? 2 : 3;
			}
			return -1;
		}
	}
}