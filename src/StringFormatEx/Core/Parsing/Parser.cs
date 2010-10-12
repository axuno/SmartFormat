using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Parsing;

namespace StringFormatEx.Core.Parsing
{
    public class Parser
    {
        #region: Extra Selector Chars :

        private string extraSelectorChars = "";
        /// <summary>
        /// Allows you to extend the allowable selector characters,
        /// to support additional selector syntaxes.
        /// </summary>
        /// <param name="chars"></param>
        public void AddExtraSelectorChars(string chars)
        {
            extraSelectorChars += chars;
        }
        
        #endregion

        #region: Watched Chars :

        private string watchedChars = "";
        public void AddWatchedChar(char c)
        {
            this.watchedChars += c;
        }

        #endregion

        #region: Parsing :

        public Format ParseFormat(string format)
        {
            var result = new Format(format);
            var current = result;
            Placeholder currentPlaceholder = null;

            int lastI = 0;
            for (int i = 0, length = format.Length; i < length; i++)
            {
                var c = format[i];
                if (currentPlaceholder == null)
                {
                    if (c == '{')
                    {
                        // Finish the last text item:
                        if (i != lastI)
                            current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
                        lastI = i + 1;

                        // See if this brace should be escaped:
                        var nextI = i + 1;
                        if (nextI < length && format[nextI] == '{')
                        {
                            i++;
                            continue;
                        }

                        // New placeholder:
                        currentPlaceholder = new Placeholder(current, i);
                        current.Items.Add(currentPlaceholder);
                        current.HasNested = true;
                    }
                    else if (c == '}')
                    {
                        // Finish the last text item:
                        if (i != lastI)
                            current.Items.Add(new LiteralText(current, lastI) { endIndex = i });
                        lastI = i + 1;

                        // See if this brace should be escaped:
                        var nextI = i + 1;
                        if (nextI < length && format[nextI] == '}')
                        {
                            i++;
                            continue;
                        }

                        // Make sure that this is a nested placeholder:
                        if (current.parent == null)
                        {
                            FormatError(format, i, "Format string is missing a closing bracket", result);
                            return result;
                        }
                        // End of the placeholder:
                        current.endIndex = i;
                        current.parent.endIndex = i + 1;
                        current = current.parent.parent;
                    }
                    else if (watchedChars.Contains(c))
                    {
                        current.AddWatchedCharacter(c, i);
                    }
                }
                else
                {
                    if (c == '.')
                    {
                        // Add the selector:
                        if (i != lastI)
                            currentPlaceholder.Selectors.Add(format.Substring(lastI, i - lastI));
                        lastI = i + 1;
                    }
                    else if (c == ':')
                    {
                        // Add the selector:
                        if (i != lastI)
                            currentPlaceholder.Selectors.Add(format.Substring(lastI, i - lastI));
                        lastI = i + 1;

                        // Start the format:
                        currentPlaceholder.Format = new Format(currentPlaceholder, i + 1);
                        current = currentPlaceholder.Format;
                        currentPlaceholder = null;
                    }
                    else if (c == '}')
                    {
                        // Add the selector:
                        if (i != lastI)
                            currentPlaceholder.Selectors.Add(format.Substring(lastI, i - lastI));
                        lastI = i + 1;

                        // End the placeholder with no format:
                        currentPlaceholder.endIndex = i + 1;
                        current = (Format)currentPlaceholder.parent;
                        currentPlaceholder = null;
                    }
                    else 
                    {
                        // Let's make sure the selector characters are valid:
                        // Make sure it's alphanumeric:
                        if (('a' <= c && c <= 'z')
                         || ('A' <= c && c <= 'Z')
                         || ('0' <= c && c <= '9')
                         || ('_' == c)
                         || (extraSelectorChars.Contains(c)))
                        { }
                        else
                        {
                            // Invalid character in the selector.
                            FormatError(format, i, "Invalid character in the selector", result);
                            return result;
                        }
                    
                    }
                }
            }

            // finish the last text item:
            if (lastI != format.Length)
                current.Items.Add(new LiteralText(current, lastI) { endIndex = format.Length });

            // Check that the format is finished:
            if (current.parent != null)
            {
                FormatError(format, format.Length, "Format string is missing a closing bracket", result);
            }

            return result;
        }

        #endregion

        #region: Errors :

        public void FormatError(string format, int index, string issue, Format formatSoFar)
        {
            throw new FormatException(format, index, issue, formatSoFar);
        }

        #endregion

    }



}
