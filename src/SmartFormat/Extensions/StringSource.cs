// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Include this source for handling <see langword="string"/>s and its extension methods.
    /// </summary>
    public class StringSource : Source
    {
        private CultureInfo _cultureInfo = CultureInfo.CurrentUICulture;

        /// <summary>
        /// CTOR.
        /// </summary>
        public StringSource() : base()
        {
            SelectorMethods =  new Dictionary<string, Func<ISelectorInfo, string, bool>>();
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of methods that can be used as selectors.
        /// </summary>
        protected Dictionary<string, Func<ISelectorInfo, string, bool>> SelectorMethods
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override void Initialize(SmartFormatter smartFormatter)
        {
            base.Initialize(smartFormatter);
            var comparer = smartFormatter.Settings.GetCaseSensitivityComparer();
            // Comparer is called when _adding_ items to the Dictionary (not, when getting items)
            SelectorMethods =  new Dictionary<string, Func<ISelectorInfo, string, bool>>(comparer);
            AddMethods();
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
            var selector = selectorInfo.SelectorText;
            _cultureInfo = GetCulture(selectorInfo.FormatDetails);
            
            // Search is case-insensitive
            if (!SelectorMethods.TryGetValue(selector, out var method)) return false;

            // Check if the Selector must match case-sensitive
            if (selectorInfo.FormatDetails.Settings.CaseSensitivity == CaseSensitivityType.CaseSensitive &&
                method.Method.Name != selector)
                return false;

            return method.Invoke(selectorInfo, currentValue);
        }

        private bool Length(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.Length;
            return true;
        }

        private bool ToUpper(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.ToUpper(_cultureInfo);
            return true;
        }

        private bool ToUpperInvariant(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.ToUpperInvariant();
            return true;
        }

        private bool ToLower(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.ToLower(_cultureInfo);
            return true;
        }

        private bool ToLowerInvariant(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.ToLowerInvariant();
            return true;
        }

        private bool Trim(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.Trim();
            return true;
        }

        private bool TrimStart(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.TrimStart();
            return true;
        }

        private bool TrimEnd(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = currentValue.TrimEnd();
            return true;
        }
        private bool ToCharArray(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result =currentValue.ToCharArray();
            return true;
        }

        private bool Capitalize(ISelectorInfo selectorInfo, string currentValue)
        {
            if (currentValue.Length < 1 || char.IsUpper(currentValue[0]))
            {
                selectorInfo.Result = currentValue;
                return true;
            }
            
            if (currentValue.Length < 2)
            {
                selectorInfo.Result = char.ToUpper(currentValue[0], _cultureInfo);
                return true;
            }
            
            selectorInfo.Result = char.ToUpper(currentValue[0], _cultureInfo) + currentValue.Substring(1);
            return true;
        }

        /// <summary>
        /// Converts the first character of each word to an uppercase character.
        /// </summary>
        private bool CapitalizeWords(ISelectorInfo selectorInfo, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                selectorInfo.Result = currentValue;
                return true;
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

            selectorInfo.Result = new string(textArray);
            return true;
        }

        private bool ToBase64(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = Convert.ToBase64String(Encoding.UTF8.GetBytes(currentValue));
            return true;
        }

        private bool FromBase64(ISelectorInfo selectorInfo, string currentValue)
        {
            selectorInfo.Result = Encoding.UTF8.GetString(Convert.FromBase64String(currentValue));
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CultureInfo GetCulture(FormatDetails formatDetails)
        {
            if (formatDetails.Provider is CultureInfo info)
                return info;

            return CultureInfo.CurrentUICulture;
        }
    }
}