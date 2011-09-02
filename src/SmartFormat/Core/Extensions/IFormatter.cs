using SmartFormat.Core.Parsing;
using SmartFormat.Core.Output;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// Converts an object to a string.
    /// </summary>
    public interface IFormatter
    {
        /// <summary>
        /// Takes the current object and writes it to the output, using the specified format.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="format"></param>
        /// <param name="handled"></param>
        /// <param name="output"></param>
        /// <param name="formatDetails"></param>
        void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails);
    }
}
