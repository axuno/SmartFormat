using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Plugins
{
    [PluginPriority(PluginPriority.Low)]
    public class DefaultFormatter : IFormatter
    {
        /// <summary>
        /// Do the default formatting, same as "String.Format".
        /// </summary>
        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            // This function always handles the method:
            handled = true;

            // If the format has nested placeholders, we process those first 
            // instead of formatting the item:
            if (format != null && format.HasNested)
            {
                formatDetails.Formatter.Format(output, format, formatDetails.OriginalArgs, current, formatDetails.FormatCache);
                return;
            }

            // If the object is null, we shouldn't write anything
            if (current == null)
            {
                return;
            }


            //  (The following code was adapted from the built-in String.Format code)

            //  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.
            var formatter = formatDetails.Formatter;
            string result = null;
            ICustomFormatter cFormatter;
            IFormattable formattable;
            // Use the provider to see if a CustomFormatter is available:
            if (formatter.Provider != null && (cFormatter = formatter.Provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter) != null)
            {
                var formatText = format == null ? null : format.GetText();
                result = cFormatter.Format(formatText, current, formatter.Provider);
            }
            // IFormattable:
            else if ((formattable = current as IFormattable) != null)
            {
                var formatText = format == null ? null : format.ToString();
                result = formattable.ToString(formatText, formatter.Provider);
            }
            // ToString:
            else
            {
                result = current.ToString();
            }


            // Now that we have the result, let's output it (and consider alignment):
            
            
            // See if there's a pre-alignment to consider:
            if (formatDetails.Placeholder.Alignment > 0)
            {
                var spaces = formatDetails.Placeholder.Alignment - result.Length;
                if (spaces > 0)
                {
                    output.Write(new String(' ', spaces));
                }
            }

            // Output the result:
            output.Write(result);


            // See if there's a post-alignment to consider:
            if (formatDetails.Placeholder.Alignment < 0)
            {
                var spaces = -formatDetails.Placeholder.Alignment - result.Length;
                if (spaces > 0)
                {
                    output.Write(new String(' ', spaces));
                }
            }
        }
    }
}
