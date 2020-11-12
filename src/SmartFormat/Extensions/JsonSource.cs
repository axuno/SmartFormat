using System;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using SmartFormat.Core.Extensions;
using System.Linq;

namespace SmartFormat.Extensions
{
    public class JsonSource : ISource
    {
        public JsonSource(SmartFormatter formatter)
        {
            // Note: We only have ONE parser at a time.
            // These settings will affect all extensions loaded at the same time

            // Escaped JSON property names and special characters are not supported in
            // order to avoid interference with other extensions
            formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
            formatter.Parser.AddAdditionalSelectorChars("_");
            // For JsonSource it would be optimal not to have any operators in place,
            // but we have a workaround, if they are set by other extensions
            formatter.Parser.AddOperators("."); 
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            // Note: Operators are processed by ListFormatter
            switch (selectorInfo.CurrentValue)
            {
                case JObject _:
                    return NewtonSoftJson.TryEvaluateSelector(selectorInfo);
                case JsonElement _:
                    return SystemTextJson.TryEvaluateSelector(selectorInfo);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluation class for <see cref="Newtonsoft.Json"/>.
        /// </summary>
        private static class NewtonSoftJson
        {
            // Note: Operators are processed by ListFormatter
            public static bool TryEvaluateSelector(ISelectorInfo selectorInfo)
            {
                var jsonObject = (JObject) selectorInfo.CurrentValue;

                var result = jsonObject.GetValue(selectorInfo.SelectorText,
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
                    targetElement = je.GetProperty(selectorInfo.SelectorText);
                }

                switch (targetElement.ValueKind) {
                    case JsonValueKind.Undefined:
                        throw new FormatException($"'{selectorInfo.SelectorText}'");
                    case JsonValueKind.Null:
                        selectorInfo.Result = null;
                        break;
                    case JsonValueKind.Number:
                        selectorInfo.Result = targetElement.GetDouble();
                        break;
                    case JsonValueKind.False:
                        selectorInfo.Result = false;
                        break;
                    case JsonValueKind.True:
                        selectorInfo.Result = true;
                        break;
                    case JsonValueKind.String:
                        selectorInfo.Result = targetElement.GetString ();
                        break;
                    case JsonValueKind.Object:
                        selectorInfo.Result = targetElement;
                        break;
                    case JsonValueKind.Array:
                        selectorInfo.Result = targetElement.EnumerateArray().ToArray();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return true;
            }
        }
    }
}