using SmartFormat.Core.Parsing;
using SmartFormat.Core.Output;

namespace SmartFormat.Core.Extensions
{
	/// <summary> Converts an object to a string. </summary>
	public interface IFormatter
	{
		/// <summary> Takes the current object and writes it to the output, using the specified format. </summary>
		void EvaluateFormat(FormattingInfo formattingInfo);

		string[] Names { get; set; }
	}
}
