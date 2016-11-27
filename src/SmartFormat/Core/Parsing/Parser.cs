using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Parsing
{
	/// <summary>
	/// Parses a format string.
	/// </summary>
	public class Parser
	{
		#region: Constructor :

		public Parser(ErrorAction errorAction = ErrorAction.Ignore)
		{
			ErrorAction = errorAction;
		}

		#endregion

		#region: Special Chars :
		
		// The following fields are points of extensibility

		/// <summary>
		/// If false, only digits are allowed as selectors.
		/// If true, selectors can be alpha-numeric.
		/// This allows optimized alpha-character detection.
		/// Specify any additional selector chars in AllowedSelectorChars.
		/// </summary>
		private bool alphanumericSelectors = false;
		/// <summary>
		/// A list of allowable selector characters,
		/// to support additional selector syntaxes such as math.
		/// Digits are always included, and letters can be included 
		/// with AlphanumericSelectors.
		/// </summary>
		private string allowedSelectorChars = "";

		/// <summary>
		/// A list of characters that come between selectors.
		/// This can be "." for dot-notation, "[]" for arrays,
		/// or even math symbols.
		/// By default, there are no operators.
		/// </summary>
		private string operators = "";

		/// <summary>
		/// If false, double-curly braces are escaped.
		/// If true, the AlternativeEscapeChar is used for escaping braces.
		/// </summary>
		private bool alternativeEscaping = false;

		/// <summary>
		/// If AlternativeEscaping is true, then this character is
		/// used to escape curly braces.
		/// </summary>
		private char alternativeEscapeChar = '\\';

		/// <summary>
		/// Includes a-z and A-Z in the list of allowed selector chars.
		/// </summary>
		public void AddAlphanumericSelectors()
		{
			alphanumericSelectors = true;
		}
		/// <summary>
		/// Adds specific characters to the allowed selector chars.
		/// </summary>
		/// <param name="chars"></param>
		public void AddAdditionalSelectorChars(string chars)
		{
			foreach (var c in chars)
			{
				if (allowedSelectorChars.IndexOf(c) == -1)
				{
					allowedSelectorChars += c;
				}
			}

		}
		/// <summary>
		/// Adds specific characters to the allowed operator chars.
		/// An operator is a character that is in the selector string
		/// that splits the selectors.
		/// </summary>
		/// <param name="chars"></param>
		public void AddOperators(string chars)
		{
			foreach (var c in chars)
			{
				if (operators.IndexOf(c) == -1)
				{
					operators += c;
				}
			}
		}

		/// <summary>
		/// Sets the AlternativeEscaping option to True 
		/// so that braces will only be escaped after the
		/// specified character.
		/// </summary>
		/// <param name="alternativeEscapeChar"></param>
		public void UseAlternativeEscapeChar(char alternativeEscapeChar)
		{
			this.alternativeEscapeChar = alternativeEscapeChar;
			this.alternativeEscaping = true;
		}
		/// <summary>
		/// [Default] 
		/// Uses {{ and }} for escaping braces for compatibility with String.Format.  
		/// However, this does not work very well with nested placeholders,
		/// so it is recommended to use an alternative escape char.
		/// </summary>
		public void UseBraceEscaping()
		{
			this.alternativeEscaping = false;
		}


		private char openingBrace = '{';
		private char closingBrace = '}';

		public void UseAlternativeBraces(char opening, char closing)
		{
			openingBrace = opening;
			closingBrace = closing;
		}

		#endregion

		#region : EventHandlers :

		/// <summary>
		/// Event raising, if an error occurs during parsing.
		/// </summary>
		public event EventHandler<ParsingErrorEventArgs> OnParsingFailure;

		#endregion

		#region: Parsing :

		public Format ParseFormat(string format, List<Extensions.IFormatter> formatterExtensions)
		{
			var result = new Format(format);
			var current = result;
			Placeholder currentPlaceholder = null;
			var namedFormatterStartIndex = -1;
			var namedFormatterOptionsStartIndex = -1;
			var namedFormatterOptionsEndIndex = -1;

			// Store parsing errors until the end:
			var parsingErrors = new ParsingErrors(result);
			var parsingErrorText = new ParsingErrorText();

			// Cache properties:
			var openingBrace = this.openingBrace;
			var closingBrace = this.closingBrace;


			int nestedDepth = 0;
			int lastI = 0;
			int operatorIndex = 0;
			int selectorIndex = 0;
			for (int i = 0, length = format.Length; i < length; i++)
			{
				var c = format[i];
				if (currentPlaceholder == null)
				{
					if (c == openingBrace)
					{
						// Finish the last text item:
						if (i != lastI)
						{
							current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
						}
						lastI = i + 1;

						// See if this brace should be escaped:
						if (!this.alternativeEscaping)
						{
							var nextI = lastI;
							if (nextI < length && format[nextI] == openingBrace)
							{
								i++;
								continue;
							}
						}

						// New placeholder:
						nestedDepth++;
						currentPlaceholder = new Placeholder(current, i, nestedDepth);
						current.Items.Add(currentPlaceholder);
						current.HasNested = true;
						operatorIndex = i+1;
						selectorIndex = 0;
						namedFormatterStartIndex = -1;
					}
					else if (c == closingBrace)
					{
						// Finish the last text item:
						if (i != lastI)
							current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
						lastI = i + 1;

						// See if this brace should be escaped:
						if (!this.alternativeEscaping)
						{
							var nextI = lastI;
							if (nextI < length && format[nextI] == closingBrace)
							{
								i++;
								continue;
							}
						}

						// Make sure that this is a nested placeholder before we un-nest it:
						if (current.parent == null)
						{
							parsingErrors.AddIssue(current, parsingErrorText[ParsingError.TooManyClosingBraces], i, i + 1);
							OnParsingFailure?.Invoke(this, new ParsingErrorEventArgs(current.RawText, i, i+1, ParsingError.TooManyClosingBraces, ErrorAction != ErrorAction.ThrowError));
							continue;
						}
						// End of the placeholder's Format:
						nestedDepth--;
						current.endIndex = i;
						current.parent.endIndex = i + 1;
						current = current.parent.parent;
						namedFormatterStartIndex = -1;
					}
					else if (this.alternativeEscaping && c == this.alternativeEscapeChar)
					{
						namedFormatterStartIndex = -1;
						// See if the next char is a brace that should be escaped:
						var nextI = i + 1;
						if (nextI < length && (format[nextI] == openingBrace || format[nextI] == closingBrace))
						{
							// Finish the last text item:
							if (i != lastI)
							{
								current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
							}
							lastI = i + 1;

							i++;
							continue;
						}
					}
					else if (namedFormatterStartIndex != -1)
					{
						if (c == '(')
						{
							var emptyName = (namedFormatterStartIndex == i);
							if (emptyName)
							{
								namedFormatterStartIndex = -1;
								continue;
							}
							namedFormatterOptionsStartIndex = i;
						}
						else if (c == ')' || c == ':')
						{
							if (c == ')')
							{
								var hasOpeningParenthesis = (namedFormatterOptionsStartIndex != -1);

								// ensure no trailing chars past ')'
								var nextI = i + 1;
								var nextCharIsValid = (nextI < format.Length && (format[nextI] == ':' || format[nextI] == closingBrace));

								if (!hasOpeningParenthesis || !nextCharIsValid)
								{
									namedFormatterStartIndex = -1;
									continue;
								}

								namedFormatterOptionsEndIndex = i;

								if (format[nextI] == ':')
								{
									i++; // Consume the ':'
								}

							}
							
							var nameIsEmpty = (namedFormatterStartIndex == i);
							var missingClosingParenthesis = (namedFormatterOptionsStartIndex != -1 && namedFormatterOptionsEndIndex == -1);
							if (nameIsEmpty || missingClosingParenthesis)
							{
								namedFormatterStartIndex = -1;
								continue;
							}


							lastI = i + 1;

							var parentPlaceholder = current.parent;

							if (namedFormatterOptionsStartIndex == -1)
							{
								var formatterName = format.Substring(namedFormatterStartIndex, i - namedFormatterStartIndex);
								
								if (FormatterNameExists(formatterName, formatterExtensions))
								{
									parentPlaceholder.FormatterName = formatterName;
								}
								else
								{
									lastI = current.startIndex;
								}

							}
							else
							{
								var formatterName = format.Substring(namedFormatterStartIndex, namedFormatterOptionsStartIndex - namedFormatterStartIndex);

								if (FormatterNameExists(formatterName, formatterExtensions))
								{
									parentPlaceholder.FormatterName = formatterName;
									parentPlaceholder.FormatterOptions = format.Substring(namedFormatterOptionsStartIndex + 1, namedFormatterOptionsEndIndex - (namedFormatterOptionsStartIndex + 1));
								}
								else
								{
									lastI = current.startIndex;
								}
							}
							current.startIndex = lastI;

							namedFormatterStartIndex = -1;
						}
					}
				}
				else
				{
					// Placeholder is NOT null, so that means 
					// we're parsing the selectors:
					if (operators.IndexOf(c) != -1)
					{
						// Add the selector:
						if (i != lastI)
						{   
							currentPlaceholder.Selectors.Add(new Selector(format, lastI, i, operatorIndex, selectorIndex));
							selectorIndex++;
							operatorIndex = i;
						}

						lastI = i + 1;
					}
					else if (c == ':')
					{
						// Add the selector:
						if (i != lastI)
						{
							currentPlaceholder.Selectors.Add(new Selector(format, lastI, i, operatorIndex, selectorIndex));
						}
						else if (operatorIndex != i)
						{
							// There are trailing operators. For now, this is an error.
							parsingErrors.AddIssue(current, parsingErrorText[ParsingError.TrailingOperatorsInSelector], operatorIndex, i);
							OnParsingFailure?.Invoke(this, new ParsingErrorEventArgs(current.RawText, operatorIndex, i + 1, ParsingError.TrailingOperatorsInSelector, ErrorAction != ErrorAction.ThrowError));
						}
						lastI = i + 1;

						// Start the format:
						currentPlaceholder.Format = new Format(currentPlaceholder, i + 1);
						current = currentPlaceholder.Format;
						currentPlaceholder = null;
						namedFormatterStartIndex = lastI;
						namedFormatterOptionsStartIndex = -1;
						namedFormatterOptionsEndIndex = -1;
					}
					else if (c == closingBrace)
					{
						// Add the selector:
						if (i != lastI)
							currentPlaceholder.Selectors.Add(new Selector(format, lastI, i, operatorIndex, selectorIndex));
						else if (operatorIndex != i)
						{
							// There are trailing operators.  For now, this is an error.
							parsingErrors.AddIssue(current, parsingErrorText[ParsingError.TrailingOperatorsInSelector], operatorIndex, i);
							OnParsingFailure?.Invoke(this, new ParsingErrorEventArgs(current.RawText, operatorIndex, i, ParsingError.TrailingOperatorsInSelector, ErrorAction != ErrorAction.ThrowError));
						}
						lastI = i + 1;

						// End the placeholder with no format:
						nestedDepth--;
						currentPlaceholder.endIndex = i + 1;
						current = currentPlaceholder.parent;
						currentPlaceholder = null;
					}
					else
					{
						// Let's make sure the selector characters are valid:
						// Make sure it's alphanumeric:
						var isValidSelectorChar =
							('0' <= c && c <= '9')
							|| (alphanumericSelectors && ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z'))
							|| (allowedSelectorChars.IndexOf(c) != -1);
						if (!isValidSelectorChar)
						{
							// Invalid character in the selector.
							parsingErrors.AddIssue(current, parsingErrorText[ParsingError.InvalidCharactersInSelector], i, i + 1);
							OnParsingFailure?.Invoke(this, new ParsingErrorEventArgs(current.RawText, i, i + 1, ParsingError.InvalidCharactersInSelector, ErrorAction != ErrorAction.ThrowError));
						}
					}
				}
			}

			// finish the last text item:
			if (lastI != format.Length)
				current.Items.Add(new LiteralText(current, lastI) { endIndex = format.Length });

			// Check that the format is finished:
			if (current.parent != null || currentPlaceholder != null)
			{
				parsingErrors.AddIssue(current, parsingErrorText[ParsingError.MissingClosingBrace], format.Length, format.Length);
				OnParsingFailure?.Invoke(this, new ParsingErrorEventArgs(current.RawText, format.Length, format.Length, ParsingError.MissingClosingBrace, ErrorAction != ErrorAction.ThrowError));
				current.endIndex = format.Length;
				while (current.parent != null)
				{
					current = current.parent.parent;
					current.endIndex = format.Length;
				}
			}

			// Check if there were any parsing errors:
			if (parsingErrors.HasIssues && ErrorAction == ErrorAction.ThrowError) throw parsingErrors;

			return result;
		}

		private bool FormatterNameExists(string name, IList<Extensions.IFormatter> formatterExtensions)
		{
			foreach (var extension in formatterExtensions)
			{
				if (extension.Names.Any(n => n != string.Empty && n == name))
					return true;
			}

			return false;
		}


		#endregion

		#region: Errors :

		public ErrorAction ErrorAction { get; set; }

		public enum ParsingError
		{
			TooManyClosingBraces = 1,
			TrailingOperatorsInSelector,
			InvalidCharactersInSelector,
			MissingClosingBrace
		}

		public class ParsingErrorText
		{
			private readonly Dictionary<ParsingError, string> _errors = new Dictionary<ParsingError, string>()
			{
				{ParsingError.TooManyClosingBraces, "Format string has too many closing braces"},
				{ParsingError.TrailingOperatorsInSelector, "There are trailing operators in the selector" },
				{ParsingError.InvalidCharactersInSelector, "Invalid character in the selector" },
				{ParsingError.MissingClosingBrace, "Format string is missing a closing brace" }
			};

			/// <summary>
			/// CTOR.
			/// </summary>
			public ParsingErrorText()
			{}

			/// <summary>
			/// Gets the string representation of the ParsingError enum.
			/// </summary>
			/// <param name="parsingErrorKey"></param>
			/// <returns>The string representation of the ParsingError enum</returns>
			public string this[ParsingError parsingErrorKey] { get { return _errors[parsingErrorKey]; }
			}
		}

		#endregion
	}
}
