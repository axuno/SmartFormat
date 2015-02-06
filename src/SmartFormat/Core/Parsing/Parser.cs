namespace SmartFormat.Core.Parsing
{
    /// <summary>
    /// Parses a format string.
    /// </summary>
    public class Parser
    {

        #region: Constructor :

        public Parser(ErrorAction errorAction)
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

        #region: Parsing :

        public Format ParseFormat(string format)
        {
            var result = new Format(format);
            var current = result;
            Placeholder currentPlaceholder = null;

            // Store parsing errors until the end:
            var parsingErrors = new ParsingErrors(result);

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
                            current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
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
                            parsingErrors.AddIssue(current, "Format string has too many closing braces", i, i + 1);
                            continue;
                        }
                        // End of the placeholder's Format:
                        nestedDepth--;
                        current.endIndex = i;
                        current.parent.endIndex = i + 1;
                        current = current.parent.parent;
                    }
                    else if (this.alternativeEscaping && c == this.alternativeEscapeChar)
                    {
                        // See if the next char is a brace that should be escaped:
                        var nextI = i + 1;
                        if (nextI < length && (format[nextI] == openingBrace || format[nextI] == closingBrace))
                        {
                            // Finish the last text item:
                            if (i != lastI)
                                current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
                            lastI = i + 1;

                            i++;
                            continue;
                        }
                    }
                }
                else
                {
                    // Placeholder is NOT null, so that means we're parsing the selectors:
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
                            // There are trailing operators.  For now, this is an error.
                            parsingErrors.AddIssue(current, "There are trailing operators in the selector", operatorIndex, i);
                        }
                        lastI = i + 1;

                        // Start the format:
                        currentPlaceholder.Format = new Format(currentPlaceholder, i + 1);
                        current = currentPlaceholder.Format;
                        currentPlaceholder = null;
                    }
                    else if (c == closingBrace)
                    {
                        // Add the selector:
                        if (i != lastI)
                            currentPlaceholder.Selectors.Add(new Selector(format, lastI, i, operatorIndex, selectorIndex));
                        else if (operatorIndex != i)
                        {
                            // There are trailing operators.  For now, this is an error.
                            parsingErrors.AddIssue(current, "There are trailing operators in the selector", operatorIndex, i);
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
                        if (('0' <= c && c <= '9')
                         || (alphanumericSelectors && ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z'))
                         || (allowedSelectorChars.IndexOf(c) != -1))
                        { }
                        else
                        {
                            // Invalid character in the selector.
                            parsingErrors.AddIssue(current, "Invalid character in the selector", i, i + 1);
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
                parsingErrors.AddIssue(current, "Format string is missing a closing brace", format.Length, format.Length);
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

        #endregion

        #region: Errors :

        public ErrorAction ErrorAction { get; set; }

        #endregion

    }
}
