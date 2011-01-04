using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat
{
    public static class SmartExtensions
    {
        #region: StringBuilder :

        /// <summary> Appends a formatted string, using the same semantics as Smart.Format. </summary>
        /// <param name="writer">The StringBuilder that will be used for output</param>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static void AppendSmart(this StringBuilder sb, string format, params object[] args)
        {
            var output = new StringOutput(sb);
            Smart.Default.FormatInto(output, format, args);
        }

        #endregion

        #region: TextWriter :

        /// <summary> Writes out a formatted string, using the same semantics as Smart.Format. </summary>
        /// <param name="writer">The TextWriter that will be used for output</param>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static void WriteSmart(this System.IO.TextWriter writer, string format, params object[] args)
        {
            var output = new TextWriterOutput(writer);
            Smart.Default.FormatInto(output, format, args);
        }

        #endregion

        #region: String :

        /// <summary> Formats the specified arguments using this string as a template. </summary>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static string FormatSmart(this string format, params object[] args)
        {
            return Smart.Format(format, args);
        }

        /// <summary> Formats the specified arguments using this string as a template. 
        /// Caches the parsing results for increased performance.
        /// </summary>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        /// <param name="cache">Outputs an object that increases performance if the same format string is used repeatedly.</param>
        public static string FormatSmart(this string format, ref FormatCache cache, params object[] args)
        {
            // With cache:
            return Smart.Default.FormatWithCache(ref cache, format, args);
        }

        #endregion

        #region: Enumerable :

        /// <summary> Formats each source item using the specified format string.
        /// Each item is separated by the separator.
        /// </summary>
        /// <param name="sources">A collection of objects to be used in formatting</param>
        /// <param name="separator">The text to insert between items</param>
        /// <param name="format">The template that defines how each source is formatted</param>
        public static string FormatAndJoin<T>(this IEnumerable<T> sources, string format, string separator)
        {
            // We're gonna need to count the items anyway, so let's not enumerate twice:
            var sourceList = sources as IList<T> ?? sources.ToList();
            var output = new StringOutput((format.Length + separator.Length) * sourceList.Count);
            var formatter = Smart.Default;
            FormatCache cache = null;
            for (int i = 0; i < sourceList.Count; i++)
            {
                if (i > 0) output.Write(separator);
                formatter.FormatWithCacheInto(ref cache, output, format, sourceList[i]);
            }
            return output.ToString();
        }

        #endregion
    }
}
