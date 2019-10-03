using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Template Formatter allows for registering reusable templates, and use them by name.
    /// </summary>
    public class TemplateFormatter : IFormatter
    {
        private readonly SmartFormatter _formatter;
        private readonly IDictionary<string, Format> _templates;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public TemplateFormatter(SmartFormatter formatter)
        {
            _formatter = formatter;

            var stringComparer = formatter.Settings.GetCaseSensitivityComparer();
            _templates = new Dictionary<string, Format>(stringComparer);
        }

        /// <summary>
        /// Gets or sets the name of the extension.
        /// </summary>
        public string[] Names { get; set; } = {"template", "t"};

        /// <summary>
        /// This method is called by the <see cref="SmartFormatter" /> to obtain the formatting result of this extension.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <returns>Returns true if successful, else false.</returns>
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var templateName = formattingInfo.FormatterOptions;
            if (templateName == "")
            {
                if (formattingInfo.Format.HasNested) return false;
                templateName = formattingInfo.Format.RawText;
            }

            if (!_templates.TryGetValue(templateName, out var template))
            {
                if (Names.Contains(formattingInfo.Placeholder.FormatterName))
                    throw new FormatException(
                        $"Formatter '{formattingInfo.Placeholder.FormatterName}' found no registered template named '{templateName}'");

                return false;
            }

            formattingInfo.Write(template, formattingInfo.CurrentValue);
            return true;
        }

        /// <summary>
        /// Register a new template.
        /// </summary>
        /// <param name="templateName">A name for the template, which is not already registered.</param>
        /// <param name="template">The string to be used as a template.</param>
        public void Register(string templateName, string template)
        {
            var parsed = _formatter.Parser.ParseFormat(template, _formatter.GetNotEmptyFormatterExtensionNames());
            _templates.Add(templateName, parsed);
        }

        /// <summary>
        /// Remove a template by its name.
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public bool Remove(string templateName)
        {
            return _templates.Remove(templateName);
        }

        /// <summary>
        /// Remove all templates.
        /// </summary>
        public void Clear()
        {
            _templates.Clear();
        }
    }
}