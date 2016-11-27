using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
	public class TemplateFormatter : IFormatter
	{
		private readonly SmartFormatter formatter;
		private readonly IDictionary<string, Format> templates;
		public TemplateFormatter(SmartFormatter formatter)
		{
			this.formatter = formatter;

			var stringComparer = (formatter.Settings.CaseSensitivity == CaseSensitivityType.CaseSensitive) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
			this.templates = new Dictionary<string, Format>(stringComparer);
		}
		public void Register(string templateName, string template)
		{
			var formatterExtensionNames = Utilities.Helper.GetNotEmptyFormatterExtensionNames(formatter.FormatterExtensions);
			var parsed = this.formatter.Parser.ParseFormat(template, formatterExtensionNames);
			this.templates.Add(templateName, parsed);
		}

		public bool Remove(string templateName)
		{
			return this.templates.Remove(templateName);
		}

		private string[] names = { "template", "t" };
		public string[] Names { get { return names; } set { names = value; } }

		public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
		{
			var templateName = formattingInfo.FormatterOptions;
			if (templateName == "")
			{
				if (formattingInfo.Format.HasNested)
				{
					return false;
				}
				templateName = formattingInfo.Format.RawText;
			}
			
			Format template;
			if (!this.templates.TryGetValue(templateName, out template))
			{
				return false;
			}

			formattingInfo.Write(template, formattingInfo.CurrentValue);
			return true;
		}

	}
}