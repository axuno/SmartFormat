// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Template Formatter allows for registering reusable templates, and use them by name.
    /// </summary>
    public class TemplateFormatter : IFormatter, IInitializer
    {
        private SmartFormatter? _formatter;
        private IDictionary<string, Format>? _templates;
        private readonly bool _canHandleAutoDetection = false;

        /// <summary>
        /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
        /// </summary>
        [Obsolete("Use property \"Name\" instead", true)]
        public string[] Names { get; set; } = {"template", "t"};

        ///<inheritdoc/>
        public string Name { get; set; } = "t";

        /// <inheritdoc/>
        /// <remarks>
        /// <see cref="TemplateFormatter"/> never can handle auto-detection.
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public bool CanAutoDetect
        {
            get
            {
                return _canHandleAutoDetection;
            }

            set
            {
                if (value) throw new ArgumentException($"{nameof(TemplateFormatter)} cannot handle auto-detection");
            }
        }

        ///<inheritdoc />
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var templateName = formattingInfo.FormatterOptions;
            if (templateName == string.Empty)
            {
                if (formattingInfo.Format is {HasNested: true}) return false;
                templateName = formattingInfo.Format?.RawText;
            }

            if (!_templates!.TryGetValue(templateName!, out var template))
            {
                throw new FormatException(
                    $"Formatter named '{formattingInfo.Placeholder?.FormatterName}' found no registered template named '{templateName}'");
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
            var parsed = _formatter!.Parser.ParseFormat(template);
            _templates!.Add(templateName, parsed);
        }

        /// <summary>
        /// Remove a template by its name.
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public bool Remove(string templateName)
        {
            return _templates!.Remove(templateName);
        }

        /// <summary>
        /// Remove all templates.
        /// </summary>
        public void Clear()
        {
            _templates!.Clear();
        }

        ///<inheritdoc/>
        public void Initialize(SmartFormatter smartFormatter)
        {
            _formatter = smartFormatter;
            var stringComparer = _formatter.Settings.GetCaseSensitivityComparer();
            _templates = new Dictionary<string, Format>(stringComparer);
        }
    }
}
