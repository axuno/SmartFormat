using System;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Do the default formatting, same logic as "String.Format".
    /// </summary>
    public class DefaultFormatter : IFormatter
    {
        /// <summary>
        /// Gets or set the names of the <see cref="DefaultFormatter"/>.
        /// </summary>
        public string[] Names { get; set; } = {"default", "d", ""};

        /// <summary>
        /// Checks, if the current value of the <see cref="ISelectorInfo"/> can be processed by the <see cref="DefaultFormatter"/>.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <returns>Returns true, if the current value of the <see cref="ISelectorInfo"/> can be processed by the <see cref="DefaultFormatter"/></returns>
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            // If the format has nested placeholders, we process those first
            // instead of formatting the item:
            if (format != null && format.HasNested)
            {
                formattingInfo.Write(format, current);
                return true;
            }

            // If the object is null, we shouldn't write anything
            if (current == null) current = "";

            // Use the provider to see if a CustomFormatter is available:
            var provider = formattingInfo.FormatDetails.Provider;

            //  (The following code was adapted from the built-in String.Format code)

            //  We will try using IFormatProvider, IFormattable, and if all else fails, ToString.
            string result;
            if (provider != null &&
                provider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter cFormatter)
            {
                var formatText = format?.GetLiteralText();
                result = cFormatter.Format(formatText, current, provider);
            }
            // IFormattable:
            else if (current is IFormattable formattable)
            {
                var formatText = format?.ToString();
                result = formattable.ToString(formatText, provider);
            }
            // ToString:
            else
            {
                result = current.ToString();
            }

            // Now that we have the result, let's output it (and consider alignment):

            // See if there's a pre-alignment to consider:
            if (formattingInfo.Alignment > 0)
            {
                var spaces = formattingInfo.Alignment - result.Length;
                if (spaces > 0) formattingInfo.Write(new string(' ', spaces));
            }

            // Output the result:
            formattingInfo.Write(result);

            // See if there's a post-alignment to consider:
            if (formattingInfo.Alignment < 0)
            {
                var spaces = -formattingInfo.Alignment - result.Length;
                if (spaces > 0) formattingInfo.Write(new string(' ', spaces));
            }

            return true;
        }
    }
}