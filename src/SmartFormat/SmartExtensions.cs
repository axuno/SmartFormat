//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System.IO;
using System.Text;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;

namespace SmartFormat
{
    /// <summary>
    /// The class contains extension methods for <see cref="StringBuilder"/>, <see cref="TextWriter"/> and <see cref="string"/>.
    /// </summary>
    public static class SmartExtensions
    {
        #region: StringBuilder :

        /// <summary> Appends a formatted string, using the same semantics as Smart.Format. </summary>
        /// <param name="sb">The StringBuilder that will be used for output</param>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static void AppendSmart(this StringBuilder sb, string format, params object[] args)
        {
            var output = new StringOutput(sb);
            Smart.Default.FormatInto(output, format, args);
        }

        /// <summary> AppendLines a formatted string, using the same semantics as Smart.Format. </summary>
        /// <param name="sb">The StringBuilder that will be used for output</param>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static void AppendLineSmart(this StringBuilder sb, string format, params object[] args)
        {
            AppendSmart(sb, format, args);
            sb.AppendLine();
        }

        #endregion

        #region: TextWriter :

        /// <summary> Writes out a formatted string, using the same semantics as Smart.Format. </summary>
        /// <param name="writer">The TextWriter that will be used for output</param>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static void WriteSmart(this TextWriter writer, string format, params object[] args)
        {
            var output = new TextWriterOutput(writer);
            Smart.Default.FormatInto(output, format, args);
        }

        /// <summary> Writes out a formatted string, using the same semantics as Smart.Format. </summary>
        /// <param name="writer">The TextWriter that will be used for output</param>
        /// <param name="format">The template that defines how the arguments are formatted</param>
        /// <param name="args">A list of arguments to be used in formatting</param>
        public static void WriteLineSmart(this TextWriter writer, string format, params object[] args)
        {
            WriteSmart(writer, format, args);
            writer.WriteLine();
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

        #endregion
    }
}