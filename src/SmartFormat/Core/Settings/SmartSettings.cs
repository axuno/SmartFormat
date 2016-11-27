using System;
using System.Collections.Generic;
using System.Reflection;

namespace SmartFormat.Core.Settings
{
	public class SmartSettings
	{
		internal SmartSettings()
		{
			CaseSensitivity = CaseSensitivityType.CaseSensitive;
		}

		/// <summary>
		/// Determines whether placeholders are case-sensitive or not.
		/// </summary>
		public CaseSensitivityType CaseSensitivity { get; set; }

		internal IEqualityComparer<string> GetCaseSensitivityComparer()
		{
			{
				switch (CaseSensitivity)
				{
					case CaseSensitivityType.CaseSensitive:
						return StringComparer.CurrentCulture;
					case CaseSensitivityType.CaseInsensitive:
						return StringComparer.CurrentCultureIgnoreCase;
					default:
						throw new InvalidOperationException($"The case sensitivity type [{CaseSensitivity}] is unknown.");
				}
			}
		}

		internal StringComparison GetCaseSensitivityComparison()
		{
			{
				switch (CaseSensitivity)
				{
					case CaseSensitivityType.CaseSensitive:
						return StringComparison.CurrentCulture;
					case CaseSensitivityType.CaseInsensitive:
						return StringComparison.CurrentCultureIgnoreCase;
					default:
						throw new InvalidOperationException($"The case sensitivity type [{CaseSensitivity}] is unknown.");
				}
			}
		}

		internal BindingFlags GetCaseSensitivityBindingFlag()
		{
			switch (CaseSensitivity)
			{
				case CaseSensitivityType.CaseSensitive:
					return 0;
				case CaseSensitivityType.CaseInsensitive:
					return BindingFlags.IgnoreCase;
				default:
					throw new InvalidOperationException($"The case sensitivity type [{CaseSensitivity}] is unknown.");
			}
		}
	}
}