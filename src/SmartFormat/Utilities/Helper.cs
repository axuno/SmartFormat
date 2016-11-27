using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Utilities
{
	public class Helper
	{
		/// <summary>
		/// Gets all names of formatter extensions which are not empty.
		/// </summary>
		/// <param name="formatterExtensions"></param>
		/// <returns></returns>
		public static string[] GetNotEmptyFormatterExtensionNames(IList<IFormatter> formatterExtensions)
		{
			var names = new List<string>();
			foreach (var extension in formatterExtensions)
			{
				names.AddRange(extension.Names.Where(n => n != string.Empty).ToArray());
			}
			return names.ToArray();
		}
	}
}
