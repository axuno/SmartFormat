using System;
using System.IO;



namespace StringFormatEx.Extensions
{
    public static class ExtendedStringFormatterExtensions
    {
        public static string FormatEx(this string format, params object[] args)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, args);
        }

        public static string FormatEx(this string format, ExtendedStringFormatter formatter, params object[] args)
        {
            return formatter.FormatEx(format, args);
        }

        public static void FormatEx(this string format, Stream output, params object[] args)
        {
            ExtendedStringFormatter.Default.FormatEx(output, format, args);
        }

        public static void FormatEx(this string format, Stream output, ExtendedStringFormatter formatter, params object[] args)
        {
            formatter.FormatEx(output, format, args);
        }

        public static void FormatEx(this string format, TextWriter output, params object[] args)
        {
            ExtendedStringFormatter.Default.FormatEx(output, format, args);
        }

        public static void FormatEx(this string format, TextWriter output, ExtendedStringFormatter formatter, params object[] args)
        {
            formatter.FormatEx(output, format, args);
        }
    }
}