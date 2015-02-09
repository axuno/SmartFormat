using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
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
		/// A flag to indicate that formatting has been handled.
		/// </summary>
		bool Handled { get; set; }

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
		/// Infrequently used details, often used for debugging
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		FormatDetails FormatDetails { get; }
	}
}