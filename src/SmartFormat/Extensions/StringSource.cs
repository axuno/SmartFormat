//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate a <see cref="Selector"/> with a <see langword="string"/> as <see cref="ISelectorInfo.CurrentValue"/>.
    /// </summary>
    public class StringSource : Source
    {
        private readonly SmartSettings _settings;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public StringSource(SmartFormatter formatter) : base(formatter)
        {
            _settings = formatter.Settings;
        }

        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            if (selectorInfo.CurrentValue is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }

            var selector = selectorInfo.SelectorText ?? string.Empty;
            if (selectorInfo.CurrentValue is not string current) return false;

            switch (selector)
            {
                // build-in string methods
                case { } when string.Equals("Length", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.Length;
                    return true;
                case { } when string.Equals("ToUpper", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.ToUpper(GetCulture(selectorInfo.FormatDetails));
                    return true;
                case { } when string.Equals("ToUpperInvariant", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.ToUpperInvariant();
                    return true;
                case { } when string.Equals("ToLower", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.ToLower(GetCulture(selectorInfo.FormatDetails));
                    return true;
                case { } when string.Equals("ToLowerInvariant", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.ToLowerInvariant();
                    return true;
                case { } when string.Equals("Trim", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.Trim();
                    return true;
                case { } when string.Equals("TrimStart", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.TrimStart();
                    return true;
                case { } when string.Equals("TrimEnd", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.TrimEnd();
                    return true;
                case { } when string.Equals("ToCharArray", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = current.ToCharArray();
                    return true;
                
                // Smart.Format methods
                case { } when string.Equals("Capitalize", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    if (current.Length < 1 || char.IsUpper(current[0]))
                        selectorInfo.Result = current;
                    else if (current.Length < 2)
                        selectorInfo.Result = char.ToUpper(current[0], GetCulture(selectorInfo.FormatDetails));
                    else
                        selectorInfo.Result = char.ToUpper(current[0], GetCulture(selectorInfo.FormatDetails)) + current.Substring(1);
                    
                    return true;
                case { } when string.Equals("CapitalizeWords", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = CapitalizeWords(current, selectorInfo);
                    return true;
                case { } when string.Equals("ToBase64", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(current));
                    return true;
                case { } when string.Equals("FromBase64", selector, selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison()):
                    selectorInfo.Result = Encoding.UTF8.GetString(Convert.FromBase64String(current));
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the first character of each word to an uppercase character.
        /// </summary>
        private static string CapitalizeWords(string text, ISelectorInfo selectorInfo)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var textArray = text.ToCharArray();
            var previousSpace = true;
            for (var i = 0; i < textArray.Length; i++)
            {
                var c = textArray[i];
                if (char.IsWhiteSpace(c))
                {
                    previousSpace = true;
                }
                else if (previousSpace && char.IsLetter(c))
                {
                    textArray[i] = char.ToUpper(c, GetCulture(selectorInfo.FormatDetails));
                    previousSpace = false;
                }
            }

            return new string(textArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CultureInfo GetCulture(FormatDetails formatDetails)
        {
            if (formatDetails.Provider is CultureInfo info)
                return info;

            return CultureInfo.CurrentCulture;
        }
    }
}