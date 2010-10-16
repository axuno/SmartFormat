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

        private readonly List<ISourcePlugin> sourcePlugins = new List<ISourcePlugin>();
        public void AddSourcePlugins(params ISourcePlugin[] sourcePlugins)
        {
            this.sourcePlugins.AddRange(sourcePlugins);
        }
        
        private readonly List<IFormatterPlugin> formatterPlugins = new List<IFormatterPlugin>();
        public void AddFormatterPlugins(params IFormatterPlugin[] formatterPlugins)
        {
            this.formatterPlugins.AddRange(formatterPlugins);
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

        public string FormatWithCache(ref Format cache, string format, params object[] args)
        {
            var output = new StringOutput(format.Length + args.Length * 8);

            if (cache == null) cache = this.Parser.ParseFormat(format);
            object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
            Format(output, cache, args, current);

            return output.ToString();
        }

        public void FormatWithCacheInto(ref Format cache, IOutput output, string format, params object[] args)
        {
            if (cache == null) cache = this.Parser.ParseFormat(format);
            object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
            Format(output, cache, args, current);
        }

        #endregion

        #region: Format :

        public void Format(IOutput output, Format format, object[] args, object current)
        {
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

                bool handled;
                // Evaluate the selectors:
                foreach (var selector in placeholder.Selectors)
                {
                    handled = false;
                    var result = context;
                    InvokeSourcePlugins(args, context, selector, ref handled, ref result);
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
                    InvokeFormatterPlugins(args, context, placeholder.Format, ref handled, output);
                }
                catch (Exception ex)
                {
                    // An error occurred while formatting.
                    var errorIndex = placeholder.Format != null ? placeholder.Format.startIndex : placeholder.Selectors.Last().endIndex;
                    FormatError(item, ex, errorIndex);
                }

            }

        }

        private void InvokeSourcePlugins(object[] args, object current, Selector selector, ref bool handled, ref object result)
        {
            foreach (var sourcePlugin in this.sourcePlugins)
            {
                sourcePlugin.EvaluateSelector(this, args, current, selector, ref handled, ref result);
                if (handled) break;
            }
        }
        private void InvokeFormatterPlugins(object[] args, object current, Format format, ref bool handled, IOutput output)
        {
            foreach (var formatterPlugin in this.formatterPlugins)
            {
                formatterPlugin.EvaluateFormat(this, args, current, format, ref handled, output);
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
