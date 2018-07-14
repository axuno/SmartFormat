using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Formatter to access part of a string.
    /// </summary>
    public class SubStringFormatter : IFormatter
    {
        /// <summary>
        /// Formatter names
        /// </summary>
        public string[] Names { get; set; } = {"substr"};

        /// <summary>
        /// The delimiter to separate parameters, defaults to comma.
        /// </summary>
        public char ParameterDelimiter { get; set; } = ',';

        /// <summary>
        /// Tries to process the given <see cref="IFormattingInfo"/>.
        /// </summary>
        /// <param name="formattingInfo">Returns true if processed, otherwise false.</param>
        /// <returns></returns>
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (formattingInfo.FormatterOptions == string.Empty) return false;
            var parameters = formattingInfo.FormatterOptions.Split(ParameterDelimiter);

            var currentValue = formattingInfo.CurrentValue?.ToString() ?? string.Empty;

            var startPos = int.Parse(parameters[0]);
            var length = parameters.Length > 1 ? int.Parse(parameters[1]) : 0;
            if (startPos < 0)
                startPos = currentValue.Length + startPos;
            if (startPos > currentValue.Length)
                startPos = currentValue.Length;
            if (length < 0)
                length = currentValue.Length - startPos + length;
            if (startPos + length > currentValue.Length)
                length = 0;
            var substring = parameters.Length > 1
                ? currentValue.Substring(startPos, length)
                : currentValue.Substring(startPos);

            formattingInfo.Write(substring);

            return true;
        }
    }
}