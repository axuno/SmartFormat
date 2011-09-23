using SmartFormat.Core.Parsing;
using SmartFormat.Core.Output;

namespace SmartFormat.Core.Extensions
{
    /// <summary> Converts an object to a string. </summary>
    public interface IFormatter
    {
        /// <summary> Takes the current object and writes it to the output, using the specified format. </summary>
        /// <param name="current"> The object to be formatted. </param>
        /// <param name="format"> Represents a parsed format string. </param>
        /// <param name="handled"> Set to indicate whether the formatter has formatted the object. </param>
        /// <param name="output"> The output to write to. </param>
        /// <param name="formatDetails"> Contains extra information about the item being formatted. </param>
        void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails);
    }
}
