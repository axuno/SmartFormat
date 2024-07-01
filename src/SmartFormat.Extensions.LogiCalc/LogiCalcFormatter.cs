using System;
using System.Collections.Generic;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.ZString;

namespace SmartFormat.Extensions;

/// <summary>
/// An <see cref="IFormatter"/> used to evaluate mathematical expressions.
/// Uses https://github.com/ncalc/ncalc
/// </summary>
/// <example>
/// var data = new { Arg1 = 3, Arg2 = 4 };
/// _ = Smart.Format("{:calc(0.00):({Arg1} + {Arg2}) * 5}");
/// result: 35.00
/// </example>
/// <remarks>
/// The <see cref="LogiCalcFormatter"/> will use plain <see cref="Placeholder"/>s as NCalc parameters.
/// NCalc parameters are useful when a value is unknown at compile time,
/// or when performance is important and repetitive parsing and compilation of the expression tree
/// can be saved for further calculations.
/// </remarks>
public class LogiCalcFormatter : IFormatter
{
    // Todo: Make the cache static and thread-safe?
    private readonly Dictionary<string, object?> _parameters = new(50);

    [ThreadStatic] // creates isolated versions of the cache dictionary in each thread
    private static Dictionary<string, (Format Format, NCalc.Domain.LogicalExpression LogExpr)>? _formatNCalcCache;

    ///<inheritdoc/>
    public string Name { get; set; } = "calc";

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = false;

    /// <summary>
    /// Contains the <see cref="NCalc.Handlers.ParameterArgs"/> created from the plain <see cref="Placeholder"/>s
    /// when the <see cref="LogiCalcFormatter"/> was invoked last.
    /// Parameters are cleared and re-created for each new format string.
    /// </summary>
    public IReadOnlyDictionary<string, object?> NCalcParameters => _parameters;

    /// <summary>
    /// Contains the functions that can be used in the NCalc expressions.
    /// </summary>
    public Dictionary<string, NCalc.ExpressionFunction> Functions { get; } = new();

    /// <summary>
    /// Contains the dynamic parameters than can be used in the NCalc expressions.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; } = new();

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        _formatNCalcCache ??= new Dictionary<string, (Format, NCalc.Domain.LogicalExpression)>(100);

        var format = formattingInfo.Format;
        if (format == null) return false;
        
        var fi = (FormattingInfo) formattingInfo;
        
        _parameters.Clear();
        using var expressionValue = CreateNCalcExpression(fi, _parameters);
        
        try
        {
            // Create Format and LogicalExpression if they don't exist for the Placeholder
            var key = formattingInfo.Placeholder!.ToString();
            if (!_formatNCalcCache.TryGetValue(key, out var cache))
            {
                cache.Format = formattingInfo.FormatDetails.Formatter.Parser.ParseFormat(
                    $"{{,{formattingInfo.Alignment}:d:{formattingInfo.FormatterOptions}}}");

                var nCalcOptions = formattingInfo.FormatDetails.Settings.CaseSensitivity == CaseSensitivityType.CaseInsensitive
                    ? NCalc.ExpressionOptions.IgnoreCaseAtBuiltInFunctions
                    : NCalc.ExpressionOptions.None;
                nCalcOptions |= NCalc.ExpressionOptions.NoCache;
                // Todo: Would this make sense?
                // nCalcOptions |= NCalc.ExpressionOptions.AllowNullParameter;

                cache.LogExpr = NCalc.Factories.LogicalExpressionFactory.Create(expressionValue.ToString(),
                    new NCalc.ExpressionContext(nCalcOptions, CultureInfo.InvariantCulture));
                
                _formatNCalcCache.Add(key, cache);
            }

            var nCalcExpression = new NCalc.Expression(cache.LogExpr)
            {
                Parameters = Parameters,
                Functions = Functions
            };

            // Add the parameters from the Placeholder
            // Don't mix placeholder parameters with user-defined parameters
            foreach (var keyValuePair in _parameters)
            {
                nCalcExpression.Parameters.Add(keyValuePair);
            }

            var result = nCalcExpression.Evaluate();
            formattingInfo.FormatAsChild(cache.Format, result);
        }
        catch (Exception exception)
        {
            throw new FormattingException(format, exception, format.StartIndex);
        }
        
        return true;
    }

    /// <summary>
    /// Creates an NCalc expression from the <see cref="FormattingInfo"/>.
    /// </summary>
    /// <param name="fi"></param>
    /// <param name="parameters"></param>
    /// <returns>
    /// The <see cref="NCalc.Expression"/> as a <see cref="ZCharArray"/>.
    /// <see cref="ZCharArray"/> must be disposed after use.
    /// </returns>
    private static ZCharArray CreateNCalcExpression(FormattingInfo fi, Dictionary<string, object?> parameters)
    {
        var buffer = new ZCharArray(ZCharArray.DefaultBufferCapacity);

        foreach (var item in fi.Format!.Items)
        {
            if (item is LiteralText literalItem)
            {
                buffer.Write(literalItem.AsSpan());
                continue;
            }

            // Otherwise, the item must be a placeholder.
            var placeholder = (Placeholder) item;

            // It's not a plain Placeholder like "{DateTime.Now.Month}",
            // so we cannot use it as an NCalc parameter but use the value directly
            if (!IsPlainPlaceholder(placeholder))
            {
                // Format the placeholder
                using var resultSpan = fi.FormatAsSpan(CultureInfo.InvariantCulture, placeholder, fi.CurrentValue);

                buffer.Write(resultSpan.GetSpan());
                continue;
            }

            // Use the Placeholder's selector names as the NCalc parameter
            // Note 1: The name of the "{}" placeholder is the empty string
            // Note 2: The selector names are joined using "dot notation", e.g. "{Person.Siblings[0]}" => "Person.Siblings.0"
            using var pNameArray = GetSelectorsDotNotation(placeholder);
            var pName = pNameArray.GetSpan();
            // NCalc does not allow empty or blank as parameter name,
            // so we use a replacement that is not a valid C# identifier

            var pNameString = pName.Length > 0 ? pName.ToString() : ".";
#if NETSTANDARD2_1 || NET6_0_OR_GREATER
            if (fi.TryGetValue(placeholder, out var result))
                parameters.TryAdd(pNameString, result);
#else
            if (!parameters.ContainsKey(pNameString))
            {
                if (fi.TryGetValue(placeholder, out var result))
                    parameters.Add(pNameString, result);
            }
#endif
            // Format as NCalc parameter
            // Parameters are useful when a value is unknown at compile time,
            // or when performance is important (NCalc parsing can be saved for further calculations).
            buffer.Write('[');
            buffer.Write(pNameString);
            buffer.Write(']');
        }

        return buffer;
    }

    /// <summary>
    /// Returns <see langword="true"/>, if the <see cref="Placeholder"/> is a plain placeholder without any formatting.
    /// This means, that the <see cref="Placeholder.FormatterName"/> is empty, the <see cref="Placeholder.FormatterOptions"/> is empty, and the <see cref="Format"/> is not nested.
    /// Alignment is allowed.
    /// <example>Example: "{DateTime.Now.Month}" or "{Invoice.Total,-10}"</example>
    /// </summary>
    private static bool IsPlainPlaceholder(Placeholder ph)
    {
        return ph.FormatterName.Length == 0 && ph.FormatterOptions.Length == 0 && ph.Format is not { HasNested: true };
    }

    /// <summary>
    /// Gets the <see cref="Selector"/>s of the <see cref="Placeholder"/> joined with the selector operator.
    /// </summary>
    /// <example>Example: For the placeholder "{Person?.Siblings[0],10}", the result is "Person.Siblings.0"</example>
    /// <returns>
    /// The <see cref="Selector"/>s of the <see cref="Placeholder"/> joined with the selector operator.
    /// <see cref="ZCharArray"/> must be disposed after use.
    /// </returns>
    private static ZCharArray GetSelectorsDotNotation(Placeholder ph)
    {
        var buffer = new ZCharArray(ph.Length * 2);

        var first = true;
#pragma warning disable S3267 // Don't use LINQ in favor of less GC
        foreach (var selector in ph.GetSelectors())
        {
            if (selector.Length == 0 || selector.Operator == ",") continue;

            if (!first)
            {
                buffer.Write('.');
            }

            buffer.Write(selector.AsSpan());
            first = false;
        }
#pragma warning restore S3267 // Restore: Loops should be simplified with "LINQ" expressions

        return buffer;
    }
}
