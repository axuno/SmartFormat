using System;
using System.IO;



namespace StringFormatEx.Extensions
{
    public static class ExtendedStringFormatterExtensions
    {
        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, params object[] args)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, args);
        }

        public static string FormatEx(this string format, object arg1)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, null, null, null, null, null, null, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, null, null, null, null, null, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, null, null, null, null, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, null, null, null, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, null, null, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, null, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, null, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7, object arg8)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, null, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7, object arg8, object arg9)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, null, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7, object arg8, object arg9, object arg10)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, null, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7, object arg8, object arg9, object arg10, object arg11)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, null, null);
        }

        public static string FormatEx(this string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, 
            object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
        {
            return ExtendedStringFormatter.Default.FormatEx(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, null);
        }

    }
}