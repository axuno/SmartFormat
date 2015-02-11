using System.ComponentModel;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
	/// <summary>
	/// Contains all necessary info for formatting a value
	/// </summary>
	/// <example>
	/// In "{Items.Length:choose(1,2,3):one|two|three}",
	/// the <see cref="CurrentValue"/> would be the value of "Items.Length",
	/// the <see cref="FormatterOptions"/> would be "1,2,3",
	/// and the <see cref="Format"/> would be "one|two|three".
	/// </example>
	public interface IFormattingInfo
	{
		/// <summary>
		/// The current value that is to be formatted.
		/// </summary>
		object CurrentValue { get; }

		/// <summary>
		/// Contains all the details about the current placeholder.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		Placeholder Placeholder { get; }

		int Alignment { get; }
		string FormatterOptions { get; }
		Format Format { get; }

		/// <summary>
		/// Writes a string to the output.
		/// </summary>
		/// <param name="text"></param>
		void Write(string text);

		/// <summary>
		/// Writes a substring to the output.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		void Write(string text, int startIndex, int length);

		/// <summary>
		/// Writes the nested format to the output.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="value"></param>
		void Write(Format format, object value);

		/// <summary>
		/// Infrequently used details, often used for debugging
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		FormatDetails FormatDetails { get; }

		/// <summary>
		/// Creates a <see cref="FormattingException"/> associated with the <see cref="IFormattingInfo.Format"/>.
		/// </summary>
		FormattingException FormattingException(string issue, FormatItem problemItem = null, int startIndex = -1);
	}
}