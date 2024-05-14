using System;
using System.Drawing;
using Demo.ThirdParty.RTFLib;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.ZString;

namespace Demo.Sample_Extensions;

public class RTFOutput : IOutput
{
    public RTFOutput(Color[] nestedColors, Color errorColor)
    {
        if (nestedColors == null || nestedColors.Length == 0) throw new ArgumentException("Nested colors cannot be null or empty.");
        this.nestedColors = nestedColors;
        this.errorColor = errorColor;
    }

    private readonly Color[] nestedColors;

    private RTFBuilder output = new RTFBuilder();
    private Color errorColor;

    public void Clear()
    {
        //output.Clear();
        output = new RTFBuilder();
    }

    public void Write(string text, IFormattingInfo? formattingInfo)
    {
        if (formattingInfo == null) return;

        Write(text, 0, text.Length, formattingInfo);
    }

    public void Write(ReadOnlySpan<char> text, IFormattingInfo? formattingInfo)
    {
        if (formattingInfo == null) return;

        Write(text.ToString(), 0, text.Length, formattingInfo);
    }

    public void Write(string text, int startIndex, int length, IFormattingInfo formattingInfo)
    {
        // Depending on the nested level, we will color this item differently:
        if (formattingInfo.FormatDetails.FormattingException != null)
        {
            output.BackColor(errorColor).Append(text, startIndex, length);
        }
        else if (formattingInfo.Placeholder == null)
        {
            // There is no "nesting" so just output plain text:
            output.Append(text, startIndex, length);
        }
        else
        {
            var nestedDepth = formattingInfo.Placeholder.NestedDepth;
            var backcolor = nestedColors[nestedDepth % nestedColors.Length];
            output.BackColor(backcolor).Append(text, startIndex, length);
        }
    }

    ///<inheritdoc/>
    public void Write(ZStringBuilder stringBuilder, IFormattingInfo? formattingInfo)
    {
        output.Append(stringBuilder.ToString());
    }
        
    public override string ToString()
    {
        return output.ToString();
    }
}
