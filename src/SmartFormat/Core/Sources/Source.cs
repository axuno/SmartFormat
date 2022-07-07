// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Extensions;

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
    public abstract bool TryEvaluateSelector(ISelectorInfo selectorInfo);

    /// <inheritdoc />
    public virtual void Initialize(SmartFormatter smartFormatter)
    {
        _formatter = smartFormatter;
        _smartSettings = smartFormatter.Settings;
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
    private bool HasNullableOperator(ISelectorInfo selectorInfo)
    {
        if (_smartSettings != null && selectorInfo.Placeholder != null)
        {
            foreach (var s in selectorInfo.Placeholder.Selectors)
            {
                if (s.OperatorLength > 1 && s.BaseString[s.OperatorStartIndex] == _smartSettings.Parser.NullableOperator)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// If any of the <see cref="Placeholder"/>'s <see cref="Placeholder.Selectors"/> has
    /// nullable <c>?</c> as their first operator, and <see cref="ISelectorInfo.CurrentValue"/>
    /// is <see langword="null"/>, <see cref="ISelectorInfo.Result"/> will be set to <see langword="null"/>.
    /// </summary>
    /// <param name="selectorInfo"></param>
    /// <returns>
    /// <see langword="true"/>, if any of the <see cref="Placeholder"/>'s
    /// <see cref="Placeholder.Selectors"/> has  nullable <c>?</c> as their first
    /// operator, and <see cref="ISelectorInfo.CurrentValue"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// The nullable operator '?' can be followed by a dot (like '?.') or a square brace (like '.[')
    /// </remarks>
    protected virtual bool TrySetResultForNullableOperator(ISelectorInfo selectorInfo)
    {
        if (HasNullableOperator(selectorInfo) && selectorInfo.CurrentValue is null)
        {
            selectorInfo.Result = null;
            return true;
        }

        return false;
    }
}
