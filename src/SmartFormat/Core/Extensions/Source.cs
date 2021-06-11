//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// The base class for <see cref="ISource"/> extension classes.
    /// </summary>
    public abstract class Source : ISource
    {
        /// <summary>
        /// The instance of the current <see cref="SmartFormatter"/>.
        /// </summary>
        protected readonly SmartFormatter _formatter;

        /// <summary>
        /// The operator character used to indicate <c>nullable</c>.
        /// </summary>
        protected readonly char _nullableOperator;

        /// <summary>
        /// The general operator character used to separate <see cref="Selector"/>s.
        /// </summary>
        protected readonly char _selectorOperator;

        /// <inheritdoc cref="ISource" />
        protected Source(SmartFormatter formatter)
        {
            _formatter = formatter;
            _nullableOperator = formatter.Settings.Parser.NullableOperator;
            _selectorOperator = formatter.Settings.Parser.SelectorOperator;
        }

        /// <inheritdoc />
        public virtual bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            return false;
        }

        /// <summary>
        /// Checks if any of the <see cref="Placeholder"/>'s <see cref="Placeholder.Selectors"/> has nullable <c>?</c> as their first operator.
        /// </summary>
        /// <param name="selectorInfo"></param>
        /// <returns>
        /// <see langword="true"/>, any of the <see cref="Placeholder"/>'s <see cref="Placeholder.Selectors"/> has nullable <c>?</c> as their first operator.
        /// </returns>
        /// <remarks>
        /// The nullable operator '?' can be followed by a dot (like '?.') or a square brace (like '.[')
        /// </remarks>
        protected virtual bool HasNullableOperator(ISelectorInfo selectorInfo)
        {
            return selectorInfo.Placeholder != null &&
                   selectorInfo.Placeholder.Selectors.Any(s =>
                       s.OperatorLength > 1 && s.BaseString[s.OperatorStartIndex] == _nullableOperator);
        }
    }
}
