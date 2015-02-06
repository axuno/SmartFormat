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
		public int optionsStartIndex;
		public int optionsEndIndex;

		public NamedFormatter(string baseString, int startIndex, int endIndex)
			: base(baseString, startIndex, endIndex)
		{
			this.optionsStartIndex = -1;
		}

		public string Name
		{
			get
			{
				var endOfName = endIndex;
				if (this.optionsStartIndex != -1)
				{
					endOfName = optionsStartIndex;
				}
				return this.baseString.Substring(startIndex, endOfName - startIndex);
			}
		}
		public string Options
		{
			get
			{
				if (this.optionsStartIndex == -1)
				{
					return "";
				}
				var startOfOptions = optionsStartIndex + 1;
				var endOfOptions = optionsEndIndex - 1;
				return this.baseString.Substring(startOfOptions, endOfOptions - startOfOptions);
			}
		}
	}
}
