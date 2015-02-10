using SmartFormat.Core.Formatting;

namespace SmartFormat.Core.Extensions
{
	/// <summary> Converts an object to a string. </summary>
	public interface IFormatter
	{
		/// <summary>
		/// An extension can be explicitly called by using any of its names.
		/// 
		/// For example, "{0:default:N2}" or "{0:d:N2}" will explicitly call the "default" extension.
		/// </summary>
		string[] Names { get; set; }

		/// <summary>
		/// Writes the current value to the output, using the specified format.
		/// 
		/// This method is only called when a "named formatter" is used.
		/// </summary>
		void EvaluateFormat(IFormattingInfo formattingInfo);

		/// <summary>
		/// Writes the current value to the output, using the specified format.
		/// 
		/// This method is only called when a "named formatter" is NOT used.
		/// IF this extension cannot write the value, returns false, otherwise true.
		/// </summary>
		bool TryEvaluateFormat(IFormattingInfo formattingInfo);
	}
}
