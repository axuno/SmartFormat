using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFormat.Core.Parsing
{
	/// <summary>
	/// Represents a named formatter, that can have optional options.
	/// </summary>
	public class NamedFormatter : FormatItem
	{
		private int optionsIndex;
		public NamedFormatter(string baseString, int startIndex, int endIndex, int optionsIndex = -1)
			: base(baseString, startIndex, endIndex)
		{
			this.optionsIndex = optionsIndex;
		}

		public string FormatterName
		{
			get
			{
				var endOfName = endIndex;
				if (this.optionsIndex != -1)
				{
					endOfName = optionsIndex;
				}
				return this.baseString.Substring(startIndex, endOfName - startIndex);
			}
		}
		public string Options
		{
			get
			{
				if (this.optionsIndex == -1)
				{
					return "";
				}
				var startOfOptions = optionsIndex + 1;
				var endOfOptions = endIndex - 1;
				return this.baseString.Substring(startOfOptions, endOfOptions - startOfOptions);
			}
		}
	}
}
