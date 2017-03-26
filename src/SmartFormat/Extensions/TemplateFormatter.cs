using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
    public class TemplateFormatter : IFormatter
    {
        private readonly SmartFormatter _formatter;
        private readonly IDictionary<string, Format> _templates;
        public TemplateFormatter(SmartFormatter formatter)
        {
            _formatter = formatter;

            var stringComparer = (formatter.Settings.CaseSensitivity == CaseSensitivityType.CaseSensitive) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            _templates = new Dictionary<string, Format>(stringComparer);
        }
        public void Register(string templateName, string template)
        {
            var parsed = _formatter.Parser.ParseFormat(template, _formatter.GetNotEmptyFormatterExtensionNames());
            _templates.Add(templateName, parsed);
        }

        public bool Remove(string templateName)
        {
            return _templates.Remove(templateName);
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
            if (!_templates.TryGetValue(templateName, out template))
            {
                return false;
            }

            formattingInfo.Write(template, formattingInfo.CurrentValue);
            return true;
        }

    }
}