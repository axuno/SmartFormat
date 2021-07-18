//
// Copyright (C) axuno gGmbH, Scott Rippey, Bernhard Millauer and other contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Extensions
{
    /// <summary>
    /// The base class for <see cref="ISource"/> extension classes.
    /// </summary>
    public abstract class Source : ISource, IInitializer
    {
        /// <summary>
        /// The instance of the current <see cref="SmartFormatter"/>.
        /// </summary>
        protected SmartFormatter? _formatter;

        /// <summary>
        /// The instance of the current <see cref="SmartSettings"/>.
        /// </summary>
        protected SmartSettings? _smartSettings;

        /// <inheritdoc />
        public virtual bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual void Initialize(SmartFormatter formatter)
        {
            _formatter = formatter;
            _smartSettings = formatter.Settings;
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
            return _smartSettings != null && selectorInfo.Placeholder != null &&
                   selectorInfo.Placeholder.Selectors.Any(s =>
                       s.OperatorLength > 1 && s.BaseString[s.OperatorStartIndex] == _smartSettings.Parser.NullableOperator);
        }
    }
}
