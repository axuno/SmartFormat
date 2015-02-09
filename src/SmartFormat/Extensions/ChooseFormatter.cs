using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
	public class ChooseFormatter : IFormatter
	{
		private string[] names = { "choose", "c" };
		public string[] Names { get { return names; } set { names = value; } }

		public void EvaluateFormat(IFormattingInfo formattingInfo)
		{
			var options = formattingInfo.FormatterOptions.Split(',');


		}
	}
}