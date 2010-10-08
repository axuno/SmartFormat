using System;
using System.IO;
using System.Text;
using StringFormatEx.Plugins.Core;



namespace StringFormatEx.Extensions
{
    public static class ExtendedStringFormatterExtensions
    {
        public static string FormatEx(this string format, IFormatProvider formatProvider, params object[] args)
        {
             StringWriter output = new StringWriter(new StringBuilder((format.Length * 2)));
            //  Guessing a length can help performance a little.
            ExtendedStringFormatter.Default.FormatExInternal(new CustomFormatInfo(ExtendedStringFormatter.Default, output, formatProvider, format, args));
            return output.ToString();
        }

        public static string FormatEx(this string format, params object[] args)
        {
            return FormatEx(format, (IFormatProvider)null, format, args);
        }


        public static void FormatEx(this string format, Stream output, IFormatProvider formatProvider, params object[] args)
        {
           ExtendedStringFormatter.Default.FormatExInternal(new CustomFormatInfo(ExtendedStringFormatter.Default, new StreamWriter(output), formatProvider, format, args));
        }
        
        public static void FormatEx(this string format, Stream output, params object[] args)
        {
            FormatEx(format, output, null, format, args);
        }


        public static void FormatEx(this string format, TextWriter output, IFormatProvider formatProvider, params object[] args)
        {
           ExtendedStringFormatter.Default.FormatExInternal(new CustomFormatInfo(ExtendedStringFormatter.Default, output, formatProvider, format, args));
        }
        
        public static void FormatEx(this string format, TextWriter output, params object[] args)
        {
            FormatEx(format, output, null, format, args);
        }


        public static void FormatEx(this string format, StringBuilder output, IFormatProvider formatProvider, params object[] args)
        {
           ExtendedStringFormatter.Default.FormatExInternal(new CustomFormatInfo(ExtendedStringFormatter.Default, new StringWriter(output), formatProvider, format, args));
        }
        
        public static void FormatEx(this string format, StringBuilder output, params object[] args)
        {
            FormatEx(format, output, null, format, args);
        }

    }
}