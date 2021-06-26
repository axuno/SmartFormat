//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
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
        private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public StringSource(SmartFormatter formatter) : base(formatter)
        {
            var comparer = new SmartSettings {CaseSensitivity = CaseSensitivityType.CaseInsensitive}
                .GetCaseSensitivityComparer();
            SelectorMethods =  new Dictionary<string, Func<ISelectorInfo, string, (bool Evaluated, object? Result)>>(comparer);
            AddMethods();
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of methods that can be used as selectors.
        /// </summary>
        protected Dictionary<string, Func<ISelectorInfo, string, (bool Evaluated, object? Result)>> SelectorMethods
        {
            get;
        }

        private void AddMethods()
        {
            // built-in string methods
            SelectorMethods.Add(nameof(Length), Length);
            SelectorMethods.Add(nameof(ToUpper), ToUpper);
            SelectorMethods.Add(nameof(ToUpperInvariant), ToUpperInvariant);
            SelectorMethods.Add(nameof(ToLower), ToLower);
            SelectorMethods.Add(nameof(ToLowerInvariant), ToLowerInvariant);
            SelectorMethods.Add(nameof(Trim), Trim);
            SelectorMethods.Add(nameof(TrimStart), TrimStart);
            SelectorMethods.Add(nameof(TrimEnd), TrimEnd);
            SelectorMethods.Add(nameof(ToCharArray), ToCharArray);
            // Smart.Format string methods
            SelectorMethods.Add(nameof(Capitalize), Capitalize);
            SelectorMethods.Add(nameof(CapitalizeWords), CapitalizeWords);
            SelectorMethods.Add(nameof(ToBase64), ToBase64);
            SelectorMethods.Add(nameof(FromBase64), FromBase64);
        }

        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            if (selectorInfo.CurrentValue is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }

            if (selectorInfo.CurrentValue is not string currentValue) return false;
            var selector = selectorInfo.SelectorText ?? string.Empty;
            _cultureInfo = GetCulture(selectorInfo.FormatDetails);
            
            // Search is case-insensitive
            if (!SelectorMethods.TryGetValue(selector, out var method)) return false;

            // Check if selector must match case-sensitive
            var caseComparison = selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison();
            if (selectorInfo.FormatDetails.Settings.CaseSensitivity == CaseSensitivityType.CaseSensitive && !SelectorMethods.Keys.Any(k => k.Equals(selector, caseComparison)))
                return false;
            
            var (evaluated, result) = method.Invoke(selectorInfo, currentValue);
            if (!evaluated) return false;

            selectorInfo.Result = result;
            return true;
        }

        private (bool Evaluated, object? Result) Length(ISelectorInfo _, string currentValue)
        {
            return (true, currentValue.Length);
        }

        private (bool Evaluated, object? Result) ToUpper(ISelectorInfo selectorInfo, string currentValue)
        {
            return (true, currentValue.ToUpper(_cultureInfo));
        }

        private (bool Evaluated, object? Result) ToUpperInvariant(ISelectorInfo _, string currentValue)
        {
            return (true, currentValue.ToUpperInvariant());
        }

        private (bool Evaluated, object? Result) ToLower(ISelectorInfo selectorInfo, string currentValue)
        {
            return (true, currentValue.ToLower(_cultureInfo));
        }

        private (bool Evaluated, object? Result) ToLowerInvariant(ISelectorInfo _,string currentValue)
        {
            return (true, currentValue.ToLowerInvariant());
        }

        private (bool Evaluated, object? Result) Trim(ISelectorInfo _, string currentValue)
        {
            return (true, currentValue.Trim());
        }

        private (bool Evaluated, object? Result) TrimStart(ISelectorInfo _, string currentValue)
        {
            return (true, currentValue.TrimStart());
        }

        private (bool Evaluated, object? Result) TrimEnd(ISelectorInfo _, string currentValue)
        {
            return (true, currentValue.TrimEnd());
        }
        private (bool Evaluated, object? Result) ToCharArray(ISelectorInfo _, string currentValue)
        {
            return (true, currentValue.ToCharArray());
        }

        private (bool Evaluated, object? Result) Capitalize(ISelectorInfo selectorInfo, string currentValue)
        {
            if (currentValue.Length < 1 || char.IsUpper(currentValue[0]))
                return (true, currentValue);
            
            if (currentValue.Length < 2)
                return (true, char.ToUpper(currentValue[0], _cultureInfo));
            
            return (true, char.ToUpper(currentValue[0], _cultureInfo) + currentValue.Substring(1));
        }

        /// <summary>
        /// Converts the first character of each word to an uppercase character.
        /// </summary>
        private (bool Evaluated, object? Result) CapitalizeWords(ISelectorInfo selectorInfo, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                return (true, currentValue);
            }

            var textArray = currentValue.ToCharArray();
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
                    textArray[i] = char.ToUpper(c, _cultureInfo);
                    previousSpace = false;
                }
            }

            return (true, new string(textArray));
        }

        private (bool Evaluated, object? Result) ToBase64(ISelectorInfo _, string currentValue)
        {
            return (true, Convert.ToBase64String(Encoding.UTF8.GetBytes(currentValue)));
        }

        private (bool Evaluated, object? Result) FromBase64(ISelectorInfo _, string currentValue)
        {
            return (true, Encoding.UTF8.GetString(Convert.FromBase64String(currentValue)));
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