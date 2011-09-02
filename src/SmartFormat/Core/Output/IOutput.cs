using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Output
{
    /// <summary>
    /// Writes a string to the output.
    /// </summary>
    public interface IOutput
    {
        /// <summary>
        /// Writes a string to the output.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="formatDetails"></param>
        void Write(string text, FormatDetails formatDetails);
        /// <summary>
        /// Writes a substring to the output.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="formatDetails"></param>
        void Write(string text, int startIndex, int length, FormatDetails formatDetails);
    }
}
