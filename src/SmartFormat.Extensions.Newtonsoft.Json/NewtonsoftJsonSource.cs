//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate <see cref="Newtonsoft.Json"/> JSON sources
    /// of type <see cref="JObject"/> and <see cref="JValue"/>.
    /// Include this source, if any of these types shall be used.
    /// </summary>
    public class NewtonsoftJsonSource : Source
    {
        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            // Check for nullable and null value
            var current = selectorInfo.CurrentValue switch
            {
                JObject jsonObject => jsonObject.HasValues ? jsonObject : null,
                JValue jsonValue => jsonValue.Value,
                _ => selectorInfo.CurrentValue
            };
            
            if (current is null && HasNullableOperator(selectorInfo))
            {
                selectorInfo.Result = null;
                return true;
            }

            if (current is null) return false;

            return selectorInfo.CurrentValue switch
            {
                // Note: Operators are processed by ListFormatter
                
                JObject jObject => TryEvaluateJObject(jObject, selectorInfo),
                JToken jToken => TryEvaluateJToken(jToken, selectorInfo), // JValue derives from JToken
                _ => false
            };
        }

        private static bool TryEvaluateJObject(JObject jsonObject, ISelectorInfo selectorInfo)
        {
            var jsonToken = jsonObject.GetValue(selectorInfo.SelectorText,
                selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison());

            return jsonToken is not null && TryEvaluateJToken(jsonToken, selectorInfo);
        }

        private static bool TryEvaluateJToken(JToken jsonToken, ISelectorInfo selectorInfo)
        {
            selectorInfo.Result = jsonToken.Type switch {
                JTokenType.Null => null,
                JTokenType.Boolean => jsonToken.Value<bool>(),
                JTokenType.Integer => jsonToken.Value<int>(),
                JTokenType.Float => jsonToken.Value<float>(),
                JTokenType.String => jsonToken.Value<string>(),
                JTokenType.Object => jsonToken.Value<object>(),
                JTokenType.Array => jsonToken.ToObject<List<object>>(),
                JTokenType.Date => jsonToken.Value<DateTime>(),
                JTokenType.TimeSpan => jsonToken.Value<TimeSpan>(),
                _ => jsonToken
            };
            return true;
        }
    }
}