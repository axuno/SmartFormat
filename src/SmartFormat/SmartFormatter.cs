using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat
{
	/// <summary>
	/// This class contains the Format method that constructs
	/// the composite string by invoking each extension.
	/// </summary>
	public class SmartFormatter
	{
		#region: Constructor :

		public SmartFormatter()
			#if DEBUG
			: this(ErrorAction.ThrowError)
			#else
			: this(ErrorAction.Ignore)
			#endif
		{
		}

		public SmartFormatter(ErrorAction errorAction)
		{
			this.Parser = new Parser(errorAction);
			this.ErrorAction = errorAction;
			this.SourceExtensions = new List<ISource>();
			this.FormatterExtensions = new List<IFormatter>();
		}

		#endregion

		#region: Extension Registry :

		public List<ISource> SourceExtensions { get; private set; }
		public List<IFormatter> FormatterExtensions { get; private set; }


		/// <summary>
		/// Adds each extensions to this formatter.
		/// Each extension must implement ISource, IFormatter, or both.
		/// 
		/// An exception will be thrown if the extension doesn't implement those interfaces.
		/// </summary>
		/// <param name="extensions"></param>
		[Obsolete("Please use the specific overloads of AddExtensions().")]
		public void AddExtensions(params object[] extensions)
		{
			foreach (var extension in extensions.Reverse())
			{
				// We need to filter each extension to the correct list:
				var source = extension as ISource;
				var formatter = extension as IFormatter;

				// If this object ISN'T a extension, throw an exception:
				if (source == null && formatter == null)
					throw new ArgumentException(string.Format("{0} does not implement ISource nor IFormatter.", extension.GetType().FullName), "extensions");

				if (source != null)
					SourceExtensions.Insert(0, source);
				if (formatter != null)
					FormatterExtensions.Insert(0, formatter);
			}
		}

		/// <summary>
		/// Adds each extensions to this formatter.
		/// Each extension must implement ISource.
		/// </summary>
		/// <param name="sourceExtensions"></param>
		public void AddExtensions(params ISource[] sourceExtensions)
		{
			SourceExtensions.InsertRange(0, sourceExtensions);
		}

		/// <summary>
		/// Adds each extensions to this formatter.
		/// Each extension must implement IFormatter.
		/// </summary>
		/// <param name="formatterExtensions"></param>
		public void AddExtensions(params IFormatter[] formatterExtensions)
		{
			FormatterExtensions.InsertRange(0, formatterExtensions);
		}


		/// <summary>
		/// Searches for a Source Extension of the given type, and returns it.
		/// This can be used to easily find and configure extensions.
		/// Returns null if the type cannot be found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetSourceExtension<T>() where T : class, ISource
		{
			return SourceExtensions.OfType<T>().First();
		}

		/// <summary>
		/// Searches for a Formatter Extension of the given type, and returns it.
		/// This can be used to easily find and configure extensions.
		/// Returns null if the type cannot be found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetFormatterExtension<T>() where T : class, IFormatter
		{
			return FormatterExtensions.OfType<T>().First();
		}

		#endregion

		#region: Properties :

		public Parser Parser { get; set; }
		public ErrorAction ErrorAction { get; set; }
		private SmartSettings settings;
		public SmartSettings Settings { get { return settings ?? (settings = new SmartSettings()); } }

		#endregion

		#region: Format Overloads :

		public string Format(string format, params object[] args)
		{
			return Format(null, format, args);
		}

		public string Format(IFormatProvider provider, string format, params object[] args)
		{
			var output = new StringOutput(format.Length + args.Length * 8);

			var formatParsed = Parser.ParseFormat(format);
			object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
			var formatDetails = new FormatDetails(this, formatParsed, args, null, provider, output);
			Format(formatDetails, formatParsed, current);

			return output.ToString();
		}

		public void FormatInto(IOutput output, string format, params object[] args)
		{
			var formatParsed = Parser.ParseFormat(format);
			object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
			var formatDetails = new FormatDetails(this, formatParsed, args, null, null, output);
			Format(formatDetails, formatParsed, current);
		}

		public string FormatWithCache(ref FormatCache cache, string format, params object[] args)
		{
			var output = new StringOutput(format.Length + args.Length * 8);

			if (cache == null) cache = new FormatCache(this.Parser.ParseFormat(format));
			object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
			var formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
			Format(formatDetails, cache.Format, current);

			return output.ToString();
		}

		public void FormatWithCacheInto(ref FormatCache cache, IOutput output, string format, params object[] args)
		{
			if (cache == null) cache = new FormatCache(this.Parser.ParseFormat(format));
			object current = (args != null && args.Length > 0) ? args[0] : args; // The first item is the default.
			var formatDetails = new FormatDetails(this, cache.Format, args, cache, null, output);
			Format(formatDetails, cache.Format, current);
		}

		private void Format(FormatDetails formatDetails, Format format, object current)
		{
			var formattingInfo = new FormattingInfo(formatDetails, format, current);
			Format(formattingInfo);
		}

		#endregion

		#region : EventHandlers :

		/// <summary>
		/// Event raising, if an error occurs during formatting.
		/// </summary>
		public event EventHandler<FormattingErrorEventArgs> OnFormattingFailure;

		#endregion

		#region: Format :

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Format(FormattingInfo formattingInfo)
		{
			// Before we start, make sure we have at least one source extension and one formatter extension:
			CheckForExtensions();
			foreach (var item in formattingInfo.Format.Items)
			{
				var literalItem = item as LiteralText;
				if (literalItem != null)
				{
					formattingInfo.Write(literalItem.baseString, literalItem.startIndex, literalItem.endIndex - literalItem.startIndex);
					continue;
				} 
				
				// Otherwise, the item must be a placeholder.
				var placeholder = (Placeholder)item;
				var childFormattingInfo = formattingInfo.CreateChild(placeholder);
				try
				{
					EvaluateSelectors(childFormattingInfo);
				}
				catch (Exception ex)
				{
					// An error occurred while formatting.
					var errorIndex = placeholder.Format != null ? placeholder.Format.startIndex : placeholder.Selectors.Last().endIndex;
					OnFormattingFailure?.Invoke(this, new FormattingErrorEventArgs(item.RawText, errorIndex, ErrorAction != ErrorAction.ThrowError));
					FormatError(item, ex, errorIndex, childFormattingInfo);
					continue;
				}

				try
				{
					EvaluateFormatters(childFormattingInfo);
				}
				catch (Exception ex)
				{
					// An error occurred while formatting.
					var errorIndex = placeholder.Format != null ? placeholder.Format.startIndex : placeholder.Selectors.Last().endIndex;
					FormatError(item, ex, errorIndex, childFormattingInfo);
					continue;
				}
			}
		}
		
		private void FormatError(FormatItem errorItem, Exception innerException, int startIndex, FormattingInfo formattingInfo)
		{
			switch (this.ErrorAction)
			{
				case ErrorAction.Ignore:
					return;
				case ErrorAction.ThrowError:
					throw (innerException as FormattingException) ?? new FormattingException(errorItem, innerException, startIndex);
				case ErrorAction.OutputErrorInResult:
					formattingInfo.FormatDetails.FormattingException = (innerException as FormattingException) ?? new FormattingException(errorItem, innerException, startIndex);
					formattingInfo.Write(innerException.Message);
					formattingInfo.FormatDetails.FormattingException = null;
					break;
				case ErrorAction.MaintainTokens:
					formattingInfo.Write(formattingInfo.Placeholder.RawText);
					break;
			}
		}

		private void CheckForExtensions()
		{
			if (this.SourceExtensions.Count == 0)
			{
				throw new InvalidOperationException("No source extensions are available.  Please add at least one source extension, such as the DefaultSource.");
			}
			if (this.FormatterExtensions.Count == 0)
			{
				throw new InvalidOperationException("No formatter extensions are available.  Please add at least one formatter extension, such as the DefaultFormatter.");
			}
		}

		private void EvaluateSelectors(FormattingInfo formattingInfo)
		{
			var placeholder = formattingInfo.Placeholder;
			var firstSelector = true;
			foreach (var selector in placeholder.Selectors)
			{
				formattingInfo.Selector = selector;
				formattingInfo.Result = null;
				var handled = InvokeSourceExtensions(formattingInfo);
				if (handled) formattingInfo.CurrentValue = formattingInfo.Result;				
				
				if (firstSelector)
				{
					firstSelector = false;
					// Handle "nested scopes" by traversing the stack:
					var parentFormattingInfo = formattingInfo;
					while (!handled && parentFormattingInfo.Parent != null)
					{
						parentFormattingInfo = parentFormattingInfo.Parent;
						parentFormattingInfo.Selector = selector;
						parentFormattingInfo.Result = null;
						handled = InvokeSourceExtensions(parentFormattingInfo);
						if (handled) formattingInfo.CurrentValue = parentFormattingInfo.Result;
					}
				}
				
				if (!handled)
				{
					throw formattingInfo.FormattingException(string.Format("Could not evaluate the selector \"{0}\"", selector.RawText), selector);
				}
			}
		}
		private bool InvokeSourceExtensions(FormattingInfo formattingInfo)
		{
			foreach (var sourceExtension in this.SourceExtensions)
			{
				var handled = sourceExtension.TryEvaluateSelector(formattingInfo);
				if (handled) return true;
			}
			return false;
		}

		private void EvaluateFormatters(FormattingInfo formattingInfo)
		{
			var handled = InvokeFormatterExtensions(formattingInfo);
			if (!handled)
			{
				throw formattingInfo.FormattingException("No suitable Formatter could be found", formattingInfo.Format);
			}
		}
		private bool InvokeFormatterExtensions(FormattingInfo formattingInfo)
		{
			var formatterName = formattingInfo.Placeholder.FormatterName;
			
			// Evaluate the named formatter (or, evaluate all "" formatters)
			foreach (var formatterExtension in this.FormatterExtensions)
			{
				if (!formatterExtension.Names.Contains(formatterName)) continue;
				var handled = formatterExtension.TryEvaluateFormat(formattingInfo);
				if (handled) return true;
			}
			return false;
		}

		#endregion
	}
}
