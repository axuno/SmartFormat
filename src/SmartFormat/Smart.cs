using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace SmartFormat
{
	/// <summary>
	/// This class holds a Default instance of the SmartFormatter.
	/// The default instance has all extensions registered.
	/// </summary>
	public static class Smart
	{
		#region: Smart.Format :

		public static string Format(string format, params object[] args)
		{
			return Default.Format(format, args);
		}

		public static string Format(IFormatProvider provider, string format, params object[] args)
		{
			return Default.Format(provider, format, args);
		}

		#endregion

		#region: Overloads - Just to match the signature of String.Format, and allow support for programming languages that don't support "params" :

		public static string Format(string format, object arg0, object arg1, object arg2)
		{
			return Format(format, new object[] { arg0, arg1, arg2 });
		}
		public static string Format(string format, object arg0, object arg1)
		{
			return Format(format, new object[] { arg0, arg1 });
		}
		public static string Format(string format, object arg0)
		{
			return Format(format, new object[] { arg0 });
		}

		#endregion

		#region: Default formatter :

		private static SmartFormatter _default;
		public static SmartFormatter Default
		{
			get
			{
				if (_default == null)
					_default = CreateDefaultSmartFormat();
				return _default;
			}
			set
			{
				_default = value;
			}
		}

		public static SmartFormatter CreateDefaultSmartFormat()
		{
			// Register all default extensions here:
			var result = new SmartFormatter();
			// Add all extensions:
			// Note, the order is important; the extensions
			// will be executed in this order:
			result.AddExtensions(
				new ListFormatter(result),
				new PluralLocalizationFormatter("en"),
				new ConditionalFormatter(),
				new TimeFormatter("en"),
				new ReflectionSource(result),
				new DictionarySource(result),
				// These default extensions reproduce the String.Format behavior:
				new DefaultSource(result),
				new DefaultFormatter()
				);

			return result;
		}

		#endregion

		#region: Settings :

		private static SmartSettings _settings;
		public static SmartSettings Settings
		{
			get
			{
				return _settings ?? (_settings = new SmartSettings());
			}
		}

		#endregion
	}
}
