using System;
using System.Collections.Generic;
using System.Reflection;

namespace SmartFormat.Core.Settings
{
	public class SmartSettings
	{
		internal SmartSettings()
		{
			CaseSensitivity = CaseSensitivityType.CaseSensitiv;
		}

		public CaseSensitivityType CaseSensitivity { get; set; }

		internal IEqualityComparer<string> GetCaseSensitivityComparer()
		{
			{
				switch (CaseSensitivity)
				{
					case CaseSensitivityType.CaseSensitiv:
						return StringComparer.CurrentCulture;
					case CaseSensitivityType.CaseInsensitiv:
						return StringComparer.CurrentCultureIgnoreCase;
					default:
						throw new InvalidOperationException(string.Format("The case sensitivity type [{0}] is unknown.", CaseSensitivity));
				}
			}
		}

		internal StringComparison GetCaseSensitivityComparision()
		{
			{
				switch (CaseSensitivity)
				{
					case CaseSensitivityType.CaseSensitiv:
						return StringComparison.CurrentCulture;
					case CaseSensitivityType.CaseInsensitiv:
						return StringComparison.CurrentCultureIgnoreCase;
					default:
						throw new InvalidOperationException(string.Format("The case sensitivity type [{0}] is unknown.", CaseSensitivity));
				}
			}
		}

		internal BindingFlags GetCaseSensitivityBindingFlag()
		{
			switch (CaseSensitivity)
			{
				case CaseSensitivityType.CaseSensitiv:
					return 0;
				case CaseSensitivityType.CaseInsensitiv:
					return BindingFlags.IgnoreCase;
				default:
					throw new InvalidOperationException(string.Format("The case sensitivity type [{0}] is unknown.", CaseSensitivity));
			}
		}
	}
}