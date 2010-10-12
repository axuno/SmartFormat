using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Plugins;
using StringFormatEx.Core.Parsing;
using StringFormatEx.Core.Output;

namespace StringFormatEx.Core
{
    /// <summary>
    /// This class contains the Format method that constructs 
    /// the composite string by invoking each plugin.
    /// </summary>
    public class SmartFormat
    {
        #region: Constructor :

        public SmartFormat()
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

        #endregion

        #region: Format Overloads :

        public string Format(string format, params object[] args)
        {
            var formatParsed = Parser.ParseFormat(format);
            var output = new StringOutput();

            Format(output, formatParsed, args);

            return output.ToString();
        }

        public void FormatInto(IOutput output, string format, params object[] args)
        {
            var formatParsed = Parser.ParseFormat(format);

            Format(output, formatParsed, args);
        }

        #endregion

        #region: Format :

        public void Format(IOutput output, Format format, params object[] args)
        {
            object current = args;
            if (args != null && args.Length > 0)
                current = args[0];

            foreach (var item in format.Items)
            {
                if (item is LiteralText)
                {
                    output.Write(item);
                    continue;
                }

                bool handled;
                var placeholder = (Placeholder)item;
                object context = current;

                // Evaluate the selectors:
                foreach (var selector in placeholder.Selectors)
                {
                    handled = false;
                    var result = context;
                    InvokeSourcePlugins(context, selector, ref handled, ref result);
                    if (!handled)
                    {
                        // The selector wasn't handled.  It's probably not a property.
                        FormatError(item, "Could not evaluate the selector: " + selector);
                    }
                    context = result;
                }

                handled = false;
                InvokeFormatterPlugins(context, placeholder.Format, ref handled, output);
                if (!handled)
                {
                    // The formatter wasn't handled.  This is unusual.
                    FormatError(item, "Could not format this item");
                }

            }

        }

        public void InvokeSourcePlugins(object arg, string selector, ref bool handled, ref object result)
        {
            foreach (var sourcePlugin in this.sourcePlugins)
            {
                sourcePlugin.EvaluateSelector(arg, selector, ref handled, ref result);
                if (handled) break;
            }
        }
        public void InvokeFormatterPlugins(object arg, Format format, ref bool handled, IOutput output)
        {
            foreach (var formatterPlugin in this.formatterPlugins)
            {
                formatterPlugin.Format(arg, format, ref handled, output);
                if (handled) break;
            }
        }

        private void FormatError(FormatItem errorItem, string issue)
        {
            throw new FormatException(errorItem.baseString, errorItem.startIndex, issue);
        }

        #endregion

    }
}
