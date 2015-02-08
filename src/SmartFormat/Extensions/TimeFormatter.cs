using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
	public class TimeFormatter : IFormatter
	{
		private string[] names = { "timespan", "time", "t" };
		public string[] Names { get { return names; } set { names = value; } }

		#region Constructors

		/// <summary>
		/// Initializes the extension with no default TimeTextInfo.
		/// </summary>
		public TimeFormatter() : this(null)
		{
		}
		/// <summary>
		/// Initializes the extension with a default TimeTextInfo.
		/// </summary>
		/// <param name="defaultTwoLetterLanguageName">This will be used when no CultureInfo is supplied.  Can be null.</param>
		public TimeFormatter(string defaultTwoLetterLanguageName)
		{
			this.DefaultFormatOptions = TimeSpanUtility.DefaultFormatOptions;
			this.DefaultTwoLetterISOLanguageName = defaultTwoLetterLanguageName;
		}

		#endregion

		#region Defaults

		public TimeSpanFormatOptions DefaultFormatOptions { get; set; }
		public string DefaultTwoLetterISOLanguageName { get; set; }

		#endregion

		#region IFormatter

		public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
		{
			if (format != null && format.HasNested) return;
			string options;
			if (formatDetails.FormatterOptions != "") 
				options = formatDetails.FormatterOptions;
			else if (format != null) 
				options = format.Text;
			else 
				options = "";
			
			TimeSpan fromTime;
			if (current is TimeSpan)
			{
				fromTime = (TimeSpan)current;
			}
			else if (current is DateTime && formatDetails.FormatterOptions != "")
			{
				fromTime = DateTime.Now.Subtract((DateTime)current);
			}
			else if (current is DateTime && options.StartsWith("timestring"))
			{
				options = options.Substring(10);
				fromTime = DateTime.Now.Subtract((DateTime)current);
			}
			else
			{
				return;
			}
			var timeTextInfo = GetTimeTextInfo(formatDetails.Provider);
			if (timeTextInfo == null)
			{
				return;
			}
			var formattingOptions = TimeSpanFormatOptionsConverter.Parse(options);
			var timeString = TimeSpanUtility.ToTimeString(fromTime, formattingOptions, timeTextInfo);
			output.Write(timeString, formatDetails);
			handled = true;

		}

		private TimeTextInfo GetTimeTextInfo(IFormatProvider provider)
		{
			if (provider != null)
			{
				// See if the provider can give us what we want:
				var timeTextInfo = (TimeTextInfo)provider.GetFormat(typeof (TimeTextInfo));
				if (timeTextInfo != null)
				{
					return timeTextInfo;
				}

				// See if there is a rule for this culture:
				var cultureInfo = provider as CultureInfo;
				if (cultureInfo != null)
				{
					timeTextInfo = CommonLanguagesTimeTextInfo.GetTimeTextInfo(cultureInfo.TwoLetterISOLanguageName);
					// If cultureInfo was supplied,
					// we will always return, even if null:
					return timeTextInfo;
				}
			}

			// Return the default if the provider couldn't provide:
			return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);
		}

		#endregion

	}

}
