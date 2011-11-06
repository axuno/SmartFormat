using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    public class DefaultFormatter : IFormatter
    {
        /// <summary>
        /// Do the default formatting, same logic as "String.Format".
        /// </summary>
        public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            // This function always handles the method:
            handled = true;

            // If the format has nested placeholders, we process those first 
            // instead of formatting the item:
            if (format != null && format.HasNested)
            {
                formatDetails.Formatter.Format(output, format, current, formatDetails);
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
            if (formatDetails.Provider != null && (cFormatter = formatDetails.Provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter) != null)
            {
                var formatText = format == null ? null : format.GetText();
                result = cFormatter.Format(formatText, current, formatDetails.Provider);
            }
            // IFormattable:
            else if ((formattable = current as IFormattable) != null)
            {
                var formatText = format == null ? null : format.ToString();
                result = formattable.ToString(formatText, formatDetails.Provider);
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
                    output.Write(new String(' ', spaces), formatDetails);
                }
            }

            // Output the result:
            output.Write(result, formatDetails);


            // See if there's a post-alignment to consider:
            if (formatDetails.Placeholder.Alignment < 0)
            {
                var spaces = -formatDetails.Placeholder.Alignment - result.Length;
                if (spaces > 0)
                {
                    output.Write(new String(' ', spaces), formatDetails);
                }
            }
        }
    }
}
