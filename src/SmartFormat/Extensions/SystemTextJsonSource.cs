//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Text.Json;
using SmartFormat.Core.Extensions;
using System.Linq;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate <see cref="System.Text.Json"/> JSON sources
    /// of type <see cref="JsonElement"/> of any <see cref="JsonValueKind"/>.
    /// Include this source, if any of this type shall be used.
    /// </summary>
    public class SystemTextJsonSource : Source
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public SystemTextJsonSource(SmartFormatter formatter) : base(formatter)
        {
        }

        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            // Check for nullable and null value
            var current = selectorInfo.CurrentValue switch
            {
                JsonElement jsonElement => jsonElement.ValueKind == JsonValueKind.Null ? null : jsonElement,
                _ => selectorInfo.CurrentValue
            };
            
            if (current is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }

            if (current is null or not JsonElement) return false;

            var je = ((JsonElement)current).Clone();

            JsonElement targetElement;
            if (selectorInfo.FormatDetails.Settings.CaseSensitivity == SmartFormat.Core.Settings.CaseSensitivityType.CaseInsensitive)
            {
                targetElement = je.EnumerateObject().FirstOrDefault(jp => jp.Name.Equals(selectorInfo.SelectorText,
                    selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison())).Value;
            }
            else
            {
                targetElement = je.GetProperty(selectorInfo.SelectorText!);
            }

            selectorInfo.Result = targetElement.ValueKind switch
            {
                JsonValueKind.Undefined => throw new FormatException($"'{selectorInfo.SelectorText}'"),
                JsonValueKind.Null => null,
                JsonValueKind.Number => targetElement.GetDouble(),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.String => targetElement.GetString(),
                JsonValueKind.Object => targetElement,
                JsonValueKind.Array => targetElement.EnumerateArray().ToArray(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return true;
        }
    }
}