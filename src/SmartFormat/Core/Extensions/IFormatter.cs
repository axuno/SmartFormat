using SmartFormat.Core.Parsing;
using SmartFormat.Core.Output;

namespace SmartFormat.Core.Extensions
{
	/// <summary> Converts an object to a string. </summary>
	public interface IFormatter
	{
		/// <summary>
		/// Takes the current object and writes it to the output, using the specified format.
		/// </summary>
		void EvaluateFormat(IFormattingInfo formattingInfo);

		/// <summary>
		/// Takes the current object and writes it to the output, using the specified format.
		/// Set the <see cref="FormattingInfo.Handled"/> flag if successful.
		/// 
		/// This is deprecated with named-formatters; use EvaluateFormat instead.
		/// </summary>
		void TryEvaluateFormat(IFormattingInfo formattingInfo);

		string[] Names { get; set; }
	}
}
