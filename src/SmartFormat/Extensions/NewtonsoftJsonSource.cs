//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
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
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public NewtonsoftJsonSource(SmartFormatter formatter) : base(formatter)
        {
        }

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
                JValue jValue => TryEvaluateJValue(jValue, selectorInfo),
                _ => false
            };
        }

        private static bool TryEvaluateJObject(JObject jsonObject, ISelectorInfo selectorInfo)
        {
            var jToken = jsonObject.GetValue(selectorInfo.SelectorText,
                selectorInfo.FormatDetails.Settings.GetCaseSensitivityComparison());

            selectorInfo.Result = jToken ?? throw new FormatException($"'{selectorInfo.SelectorText}'");
            return true;
        }

        private static bool TryEvaluateJValue(JValue jsonValue, ISelectorInfo selectorInfo)
        {
            selectorInfo.Result = jsonValue;
            return true;
        }
    }
}