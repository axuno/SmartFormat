using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Output;
using StringFormatEx.Core.Parsing;

namespace StringFormatEx.Core.Plugins
{
    public class DefaultFormatter : IFormatterPlugin
    {
        /// <summary>
        /// Do the default formatting, similar to "String.Format"
        /// </summary>
        public void EvaluateFormat(SmartFormat formatter, object[] args, object current, Format format, ref bool handled, IOutput output)
        {
            // If the object is null, we shouldn't write anything
            if (current == null)
            {
                handled = true;
                return;
            }


            //  Let's do the default formatting:
            //  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.
            //  (This code was adapted from the built-in String.Format code)
            if (formatter.Provider != null)
            {
                //  Use the provider to see if a CustomFormatter is available:
                ICustomFormatter cformatter = formatter.Provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter;
                if (cformatter != null)
                {
                    var formatText = format == null ? null : format.ToString();
                    output.Write(cformatter.Format(formatText, current, formatter.Provider));
                    return;
                }
            }

            //  Now try to format the object, using its own built-in formatting if possible:
            var formattable = current as IFormattable;
            if (formattable != null)
            {
                var formatText = format == null ? null : format.ToString();
                output.Write(formattable.ToString(formatText, formatter.Provider));
            }
            else
            {
                output.Write(current.ToString());
            }

            // This function always handles.
            handled = true;
        }
    }
}
