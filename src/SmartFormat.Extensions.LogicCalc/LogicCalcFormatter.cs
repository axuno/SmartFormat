using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

/// <summary>
/// An <see cref="IFormatter"/> used to evaluate mathematical expressions.
/// See https://github.com/ncalc/ncalc and
/// https://www.codeproject.com/Articles/18880/State-of-the-Art-Expression-Evaluation
/// </summary>
/// <example>
/// var data = new { Arg1 = 3, Arg2 = 4 };
/// _ = Smart.Format("{:M(0.00):({Arg1} + {Arg2}) * 5}");
/// result: 35.00
/// </example>
/// <remarks>
/// The <see cref="LogicCalcFormatter"/> will use un-nested <see cref="Placeholder"/>s as NCalc parameters.
/// NCalc parameters are useful when a value is unknown at compile time,
/// or when performance is important and repetitive parsing and compilation of the expression tree
/// can be saved for further calculations.
/// </remarks>
public class LogicCalcFormatter : IFormatter
{
    private readonly Dictionary<string, object?> _parameters = new(50);
    private Format? _nCalcFormat;
    private static NCalc.Expression? _nCalcExpression;
    private static readonly StringBuilder _sb = new();
    ///<inheritdoc/>
    public string Name { get; set; } = "M";

    /// <summary>
    /// Obsolete. <see cref="IFormatter"/>s only have one unique name.
    /// </summary>
    [Obsolete("Use property \"Name\" instead", true)]
    public string[] Names { get; set; } = { "M" };

    ///<inheritdoc/>
    public bool CanAutoDetect { get; set; } = false;

    /// <summary>
    /// The parameters that were used when the <see cref="LogicCalcFormatter"/> was invoked last.
    /// Can be helpful to make sure <see cref="Placeholder"/>s actually become <see cref="NCalc"/> parameters.
    /// </summary>
    public IReadOnlyDictionary<string, object?> LastNCalcParameters => _parameters;

    /// <summary>
    /// Gets or sets the <see cref="NCalc.EvaluateFunctionHandler"/>.
    /// </summary>
    public NCalc.EvaluateFunctionHandler? EvaluateFunction { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="NCalc.EvaluateParameterHandler"/>.
    /// </summary>
    public NCalc.EvaluateParameterHandler? EvaluateParameter { get; set; }

    ///<inheritdoc />
    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var format = formattingInfo.Format;
        if (format == null) return false;
        
        var fi = (FormattingInfo) formattingInfo;
        
        var nCalcOptions = formattingInfo.FormatDetails.Settings.CaseSensitivity == CaseSensitivityType.CaseInsensitive
            ? NCalc.EvaluateOptions.IgnoreCase
            : NCalc.EvaluateOptions.None;
        
        _parameters.Clear();
        var expressionValue = CreateNCalcExpression(fi, _parameters); // fi.CreateNCalcExpression(_parameters);

        _nCalcExpression = new NCalc.Expression(expressionValue, nCalcOptions, CultureInfo.InvariantCulture)
        {
            Parameters = _parameters
        };
        
        try
        {
            if (EvaluateFunction != null) _nCalcExpression.EvaluateFunction += EvaluateFunction;
            if (EvaluateParameter != null) _nCalcExpression.EvaluateParameter += EvaluateParameter;

            var result = _nCalcExpression.Evaluate();

            // Recreate the Format if Alignment or FormatterOptions have changed
            if (_nCalcFormat?.Items[0] is not Placeholder ph || ph.Alignment != formattingInfo.Alignment ||
                ph.FormatterOptions != formattingInfo.FormatterOptions)
            {
                // Creating a tailored Format for this specific case causes code duplication
                // and is only about 2.5% faster, so we use the standard Parser
                _nCalcFormat =
                    formattingInfo.FormatDetails.Formatter.Parser.ParseFormat(
                        $@"{{,{formattingInfo.Alignment}:{formattingInfo.FormatterOptions}}}");
            }

            formattingInfo.FormatAsChild(_nCalcFormat, result);
            
        }
        catch (Exception exception)
        {
            throw new FormattingException(format, exception, format.StartIndex);
        }
        
        return true;
    }

    private static string CreateNCalcExpression(FormattingInfo fi, IDictionary<string, object?> parameters)
    {
        _sb.Clear();
        _sb.Capacity = fi.Format!.Length; // default sb.Capacity is only 16
        
        foreach (var item in fi.Format.Items)
        {
            if (item is LiteralText literalItem)
            {
                _sb.Append(literalItem);
                continue;
            }

            // Otherwise, the item must be a placeholder.
            var placeholder = (Placeholder) item;

            // It's not a plain Placeholder like "{DateTime.Now.Month}":
            // We can't use it as an NCalc parameter
            if (placeholder.FormatterNameLength != 0 || placeholder.FormatterOptionsLength != 0 || placeholder.Format is { HasNested: true })
            {
                var result = fi.FormatPlaceholderToSpan(placeholder, CultureInfo.InvariantCulture, fi.CurrentValue);
                _sb.Append(result.ToString());
                continue;
            }

            // Use the Placeholder's selector names as the NCalc parameter
            var pName = string.Join(fi.FormatDetails.Settings.Parser.SelectorOperator.ToString(),
                placeholder.Selectors);
            if (!parameters.ContainsKey(pName))
            {
                parameters.Add(pName, fi.GetValue(placeholder));
            }

            // Format as NCalc parameter
            _sb.AppendFormat("[{0}]", pName);            
        }

        return _sb.ToString();
    }
}
