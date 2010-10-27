using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Plugins;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Output;

namespace SmartFormat.Core
{
    /// <summary>
    /// This class contains the Format method that constructs 
    /// the composite string by invoking each plugin.
    /// </summary>
    public class SmartFormatter
    {
        #region: Constructor :

        public SmartFormatter()
        {
            this.Parser = new Parser();
        }

        #endregion

        #region: Plugin Registration :

        private readonly List<ISource> sourcePlugins = new List<ISource>();
        private readonly List<IFormatter> formatterPlugins = new List<IFormatter>();
        /// <summary>
        /// Adds each plugins to this formatter.
        /// Each plugin must implement ISource, IFormatter, or both.
        /// 
        /// An exception will be thrown if the plugin doesn't implement those interfaces.
        /// </summary>
        /// <param name="plugins"></param>
        public void AddPlugins(params object[] plugins)
        {
            foreach (var plugin in plugins)
            {
                // We need to filter each plugin to the correct list:
                var source = plugin as ISource;
                var formatter = plugin as IFormatter;

                // If this object ISN'T a plugin, throw an exception:
                if (source == null && formatter == null)
                    throw new ArgumentException(string.Format("{0} does not implement ISource nor IFormatter.", plugin.GetType().FullName), "plugins");

                if (source != null)
                    sourcePlugins.Add(source);
                if (formatter != null)
                    formatterPlugins.Add(formatter);
            }

            // Search each plugin for the "PluginPriority" 
            // attribute, and sort the lists accordingly.

            sourcePlugins.Sort(PluginPriorityAttribute.SourceComparer());
            formatterPlugins.Sort(PluginPriorityAttribute.FormatterComparer());
        }

        #endregion

        #region: Properties :

        public Parser Parser { get; private set; }
        public IFormatProvider Provider { get; set; }

        #endregion

        #region: Format Overloads :

        public string Format(string format, params object[] args)
        {
            var output = new StringOutput(format.Length + args.Length * 8);
            
            var formatParsed = Parser.ParseFormat(format);
            object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
            Format(output, formatParsed, args, current);

            return output.ToString();
        }

        public void FormatInto(IOutput output, string format, params object[] args)
        {
            var formatParsed = Parser.ParseFormat(format);
            object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
            Format(output, formatParsed, args, current);
        }

        public string FormatWithCache(ref FormatCache cache, string format, params object[] args)
        {
            var output = new StringOutput(format.Length + args.Length * 8);

            if (cache == null) cache = new FormatCache(this.Parser.ParseFormat(format));
            object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
            Format(output, cache.Format, args, current);

            return output.ToString();
        }

        public void FormatWithCacheInto(ref FormatCache cache, IOutput output, string format, params object[] args)
        {
            if (cache == null) cache = new FormatCache(this.Parser.ParseFormat(format));
            object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
            Format(output, cache.Format, args, current);
        }

        #endregion

        #region: Format :

        public void Format(IOutput output, Format format, object[] args, object current)
        {
            var formatDetails = new FormatDetails() {
                Formatter = this,
                OriginalArgs = args,
                Placeholder = null,
            };

            foreach (var item in format.Items)
            {
                var literalItem = item as LiteralText;
                if (literalItem != null)
                {
                    output.Write(literalItem);
                    continue;
                } // Otherwise, the item is a placeholder.

                var placeholder = (Placeholder)item;
                object context = current;
                formatDetails.Placeholder = placeholder;

                bool handled;
                // Evaluate the selectors:
                foreach (var selector in placeholder.Selectors)
                {
                    handled = false;
                    var result = context;
                    InvokeSourcePlugins(context, selector, ref handled, ref result, formatDetails);
                    if (!handled)
                    {
                        // The selector wasn't handled.  It's probably not a property.
                        FormatError(selector, "Could not evaluate the selector: " + selector.Text, selector.startIndex);
                    }
                    context = result;
                }

                handled = false;
                try
                {
                    InvokeFormatterPlugins(context, placeholder.Format, ref handled, output, formatDetails);
                }
                catch (Exception ex)
                {
                    // An error occurred while formatting.
                    var errorIndex = placeholder.Format != null ? placeholder.Format.startIndex : placeholder.Selectors.Last().endIndex;
                    FormatError(item, ex, errorIndex);
                }

            }

        }

        private void InvokeSourcePlugins(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails)
        {
            foreach (var sourcePlugin in this.sourcePlugins)
            {
                sourcePlugin.EvaluateSelector(current, selector, ref handled, ref result, formatDetails);
                if (handled) break;
            }
        }
        private void InvokeFormatterPlugins(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            foreach (var formatterPlugin in this.formatterPlugins)
            {
                formatterPlugin.EvaluateFormat(current, format, ref handled, output, formatDetails);
                if (handled) break;
            }
        }

        private void FormatError(FormatItem errorItem, string issue, int startIndex)
        {
            throw new FormatException(errorItem, issue, startIndex);
        }
        private void FormatError(FormatItem errorItem, Exception innerException, int startIndex)
        {
            throw new FormatException(errorItem, innerException, startIndex);
        }

        #endregion

    }
}
