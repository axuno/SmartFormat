using System.ComponentModel;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
	/// <summary>
	/// Contains all the necessary information for evaluating a selector.
	/// </summary>
	/// <example>
	/// When evaluating "{Items.Length}", 
	/// the CurrentValue might be Items, and the Selector would be "Length".
	/// The job of an ISource is to set CurrentValue to Items.Length.
	/// </example>
	public interface ISelectorInfo
	{
		/// <summary>
		/// The current value to evaluate.
		/// </summary>
		object CurrentValue { get; }

		/// <summary>
		/// The selector to evaluate
		/// </summary>
		Selector Selector { get; }

		/// <summary>
		/// Sets the result of evaluating the selector.
		/// Be sure to also set Handled to true.
		/// </summary>
		object Result { set; }

		/// <summary>
		/// A flag to indicate that the Selector has been handled.
		/// </summary>
		/// TODO: Make this 'automatic', based on setting CurrentValue
		bool Handled { set; }

		/// <summary>
		/// Contains all the details about the current placeholder.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		Placeholder Placeholder { get; }

		/// <summary>
		/// Infrequently used details, often used for debugging
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		FormatDetails FormatDetails { get; }
	}
}