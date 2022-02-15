// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate a <see cref="Selector"/> with <see cref="KeyValuePair{TKey,TValue}"/>.
    /// The key must be <see langword="string"/>, the value must be a <see cref="Nullable{T}"/> <see cref="object"/>.
    /// </summary>
    /// <example>
    /// Smart.Format("{key}", new KeyValuePair&lt;string, object?&gt;("key", "a value");
    /// Result: "a value".
    /// </example>
    public class KeyValuePairSource : Source
    {
        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            switch (selectorInfo.CurrentValue)
            {
                case null:
                    return false;
                case KeyValuePair<string, object?> kvp when kvp.Key == selectorInfo.SelectorText:
                    selectorInfo.Result = kvp.Value;
                    return true;
                default:
                    return false;
            }
        }
    }
}
