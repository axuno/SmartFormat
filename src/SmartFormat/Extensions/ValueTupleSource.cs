//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// Class to evaluate <see cref="ValueTuple{T}"/>s.
    /// With ValueTuples
    /// a) all objects used for Smart.Format can be collected in one place as the first argument
    /// b) the format string can be written like each object would be the first argument of Smart.Format
    /// c) there is no need to bother from which argument a value should come from 
    /// </summary>
    public class ValueTupleSource : Source
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="formatter"></param>
        public ValueTupleSource(SmartFormatter formatter) : base(formatter)
        {
        }

        /// <inheritdoc />
        public override bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            if (selectorInfo is not FormattingInfo formattingInfo) return false;
            if (!(formattingInfo.CurrentValue != null && formattingInfo.CurrentValue.IsValueTuple())) return false;

            var savedCurrentValue = formattingInfo.CurrentValue;
            foreach (var obj in formattingInfo.CurrentValue.GetValueTupleItemObjectsFlattened())
            {
                formattingInfo.CurrentValue = obj;

                foreach (var sourceExtension in _formatter.SourceExtensions)
                {
                    var handled = sourceExtension.TryEvaluateSelector(formattingInfo);
                    if (handled)
                    {
                        formattingInfo.CurrentValue = savedCurrentValue;
                        return true;
                    }
                }
            }

            formattingInfo.CurrentValue = savedCurrentValue;

            return false;
        }
    }
}