using System;
using System.Collections.Generic;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx.Plugins
{
    public class _DefaultFormatPlugin : IStringFormatterPlugin
    {

        public IEnumerable<EventHandler<ExtendSourceEventArgs>> GetSourceExtensions()
        {
            return new EventHandler<ExtendSourceEventArgs>[] {};
        }

        public IEnumerable<EventHandler<ExtendFormatEventArgs>> GetFormatExtensions()
        {
            return new EventHandler<ExtendFormatEventArgs>[] 
                { _DefaultFormatPlugin._GetDefaultOutput };
        }


        /// <summary>
        /// This is the Default method for formatting the output.
        /// This code has been derived from the built-in String.Format() function.
        /// </summary>
        [CustomFormatPriority(CustomFormatPriorities.Low)]
        private static void _GetDefaultOutput(object sender, ExtendFormatEventArgs e) 
        {
            CustomFormatInfo info = e.FormatInfo;

            //  Let's see if there are nested items:
            if (info.HasNested) {
                info.CustomFormatNested();
                return;
            }

            //  Let's do the default formatting:
            //  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.
            //  (This code was adapted from the built-in String.Format code)
            if (info.Provider != null) {
                //  Use the provider to see if a CustomFormatter is available:
                ICustomFormatter formatter = info.Provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter;
                if (formatter != null) {
                    info.Write(formatter.Format(info.Format, info.Current, info.Provider));
                    return;
                }
            }

            //  Now try to format the object, using its own built-in formatting if possible:
            if (info.Current.GetType() is IFormattable) {
                info.Write(((IFormattable)info.Current).ToString(info.Format, info.Provider));
            }
            else {
                info.Write(info.Current.ToString());
            }
        }

    }
}