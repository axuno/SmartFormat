//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

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

        ///<inheritdoc />
        public string[] Names { get; set; } = {"template", "t"};

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var templateName = formattingInfo.FormatterOptions ?? string.Empty;
            if (templateName == string.Empty)
            {
                if (formattingInfo.Format is {HasNested: true}) return false;
                templateName = formattingInfo.Format?.RawText;
            }

            if (!_templates.TryGetValue(templateName!, out var template))
            {
                if (Names.Contains(formattingInfo.Placeholder?.FormatterName))
                    throw new FormatException(
                        $"Formatter '{formattingInfo.Placeholder?.FormatterName ?? "null"}' found no registered template named '{templateName}'");

                return false;
            }

            formattingInfo.FormatAsChild(template, formattingInfo.CurrentValue);
            return true;
        }

        /// <summary>
        /// Register a new template.
        /// </summary>
        /// <param name="templateName">A name for the template, which is not already registered.</param>
        /// <param name="template">The string to be used as a template.</param>
        public void Register(string templateName, string template)
        {
            var parsed = _formatter.Parser.ParseFormat(template);
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