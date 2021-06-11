//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using SmartFormat.Core.Extensions;
using System.Linq;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate <see cref="Newtonsoft.Json"/> and <see cref="System.Text.Json"/>  JSON sources
    /// of type <see cref="JObject"/> and <see cref="JsonElement"/>.
    /// </summary>
    public class JsonSource : Source
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public JsonSource(SmartFormatter formatter) : base(formatter)
        {
            // For JsonSource it would be optimal not to have any operators in place,
            // but we have a workaround, if they are set by other extensions
        }

        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            // Check for nullable and null value
            var current = selectorInfo.CurrentValue switch
            {
                // NewtonSoftJson
                JObject jsonObject => jsonObject.HasValues ? jsonObject : null,
                JValue jasonValue => jasonValue.Value,
                // System.Text.Json
                JsonElement jsonElement => jsonElement.ValueKind == JsonValueKind.Null ? null : jsonElement,
                _ => selectorInfo.CurrentValue
            };
            
            if (current is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }
            
            // Note: Operators are processed by ListFormatter
            return selectorInfo.CurrentValue switch
            {
                // NewtonSoftJson
                JObject => NewtonSoftJson.TryEvaluateSelector(selectorInfo),
                JValue => NewtonSoftJson.TryEvaluateSelector(selectorInfo),
                // System.Text.Json
                JsonElement => SystemTextJson.TryEvaluateSelector(selectorInfo),
                _ => false
            };
        }

        /// <summary>
        /// Evaluation class for <see cref="Newtonsoft.Json"/>.
        /// </summary>
        private static class NewtonSoftJson
        {
            // Note: Operators are processed by ListFormatter
            public static bool TryEvaluateSelector(ISelectorInfo selectorInfo)
            {
                var jsonObject = selectorInfo.CurrentValue as JObject;

                var result = jsonObject?.GetValue(selectorInfo.SelectorText,
                    selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison());

                selectorInfo.Result = result ?? throw new FormatException($"'{selectorInfo.SelectorText}'");
                return true;
            }
        }

        /// <summary>
        /// Evaluation class for <see cref="System.Text.Json"/>
        /// </summary>
        private static class SystemTextJson
        {
            // Note: Operators are processed by ListFormatter
            public static bool TryEvaluateSelector(ISelectorInfo selectorInfo)
            {
                if (selectorInfo.CurrentValue is null) return false;

                var jsonElement = (JsonElement) selectorInfo.CurrentValue;
            
                var je = jsonElement.Clone();
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
}