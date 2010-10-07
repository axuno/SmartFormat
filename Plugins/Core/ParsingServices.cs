using System;
using System.Collections.Generic;



namespace StringFormatEx.Plugins.Core
{
    public static class ParsingServices
    {


        #region SplitNested

        /// <summary>
        /// Enhances String.Split by ignoring any characters that are between the nested characters.
        /// It also allows you to stop after a certain number of splits.
        /// 
        /// Example:
        /// SplitNested("a|b{1|2|3}|c", "|"c) = {"a", "b{1|2|3}", "c"}
        /// SplitNested("a|b{1|2|3}|c", "|"c, 2) = {"a", "b{1|2|3}|c"}
        /// 
        /// </summary>
        public static string[] SplitNested(string format, char splitChar)
        {
            return SplitNested(format, splitChar, 0);
        }

        public static string[] SplitNested(string format, char splitChar, int maxItems)
        {
            int openCount = 0;
            List<string> items = new List<String>(4); //  (Estimating 4 matches)
            int lastSplit = 0;

            for (int i = 0; i < format.Length; i++) {
                char c = format[i];
                if (c == '{') {
                    openCount++;
                } 
                else if (c == '}') {
                    openCount--;
                } 
                else if (c == splitChar && openCount == 0) {
                    items.Add(format.Substring(lastSplit, (i - lastSplit)));
                    lastSplit = i + 1;
                    if (maxItems != 0 && items.Count == (maxItems - 1)) {
                        break;
                    }
                }
            }
            items.Add(format.Substring(lastSplit));
            return items.ToArray();
        }

        #endregion



        #region INTERNAL Shared Fields

        /// <summary>
        /// The character that escapes other characters
        /// </summary>
        internal static char escapeCharacter = '\\';
    
        /// <summary>
        /// A string that contains all the special escape characters.
        /// The order of these characters matches the order of the escapeText.
        /// </summary>
        internal static string escapeCharacters = "nt{}\\";
    
        internal static string[] escapeText = new string[] { "\r\n", "\t", "{", "}", "\\" };
    
        /// <summary>
        /// A string containing Selector split characters.
        /// Any of these characters chain together properties.
        /// For example, {Person.Address.City}.
        /// 
        /// Two things to notice:
        /// Multiple splitters in a row are ignored;
        /// Parenthesis/brackets do NOT have to be matched, and they do NOT affect order of operations.
        /// 
        /// Therefore, the following examples are identical:
        ///  {Person.Address.City} 
        ///  {Person)Address]City}
        ///  {[Person]Address(City)}
        ///  {..Person...Address...City..}
        ///  {..Person(Address)]]]City)[)..}
        /// 
        /// This allows comfortable syntaxes such as {Array[0].Item}
        /// This syntax is identical to {Array.0.Item} and {Array]0]Item} and {[Array][0](Item)}
        /// 
        /// </summary>
        internal static string selectorSplitters = ".[]()";
    
        internal static string selectorCharacters = "_";

        #endregion


    }
}