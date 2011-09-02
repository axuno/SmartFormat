using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// Evaluates a selector.
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// Takes the current object and evaluates the selector.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="selector"></param>
        /// <param name="handled"></param>
        /// <param name="result"></param>
        /// <param name="formatDetails"></param>
        void EvaluateSelector(object current, Selector selector, ref bool handled, ref object result, FormatDetails formatDetails);
    }
}
