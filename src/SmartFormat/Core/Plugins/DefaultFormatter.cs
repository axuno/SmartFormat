using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Plugins
{
    public class DefaultFormatter : IFormatterPlugin
    {
        /// <summary>
        /// Do the default formatting, same as "String.Format".
        /// </summary>
        public void EvaluateFormat(SmartFormatter formatter, object[] args, object current, Format format, ref bool handled, IOutput output)
        {
            // No matter what, this function handles the method.
            handled = true;

            // If the format has nested placeholders, we process those first 
            // instead of formatting the item:
            if (format != null && format.HasNested)
            {
                formatter.Format(output, format, args, current);
                return;
            }

            // If the object is null, we shouldn't write anything
            if (current == null)
            {
                return;
            }


            //  (The following code was adapted from the built-in String.Format code)
            
            //  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.

            // Try IFormatProvider:
            if (formatter.Provider != null)
            {
                //  Use the provider to see if a CustomFormatter is available:
                ICustomFormatter cformatter = formatter.Provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter;
                if (cformatter != null)
                {
                    var formatText = format == null ? null : format.GetText();
                    output.Write(cformatter.Format(formatText, current, formatter.Provider));
                    return;
                }
            }

            // IFormattable:
            var formattable = current as IFormattable;
            if (formattable != null)
            {
                var formatText = format == null ? null : format.ToString();
                output.Write(formattable.ToString(formatText, formatter.Provider));
            }
            // ToString:
            else
            {
                output.Write(current.ToString());
            }
        }
    }
}
