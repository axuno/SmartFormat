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
		/// <param name="selectorInfo"></param>
		void EvaluateSelector(ISelectorInfo selectorInfo);
	}
}
