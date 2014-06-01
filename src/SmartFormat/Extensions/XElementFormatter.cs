using System.Xml.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions
{
    public class XElementFormatter : IFormatter
    {
        #region IFormatter

        public void EvaluateFormat(object current, Format format,
            ref bool handled, IOutput output, FormatDetails formatDetails)
        {
            if (format != null && format.HasNested) return;
            if (current is XElement)
            {
                var currentAsXElement = current as XElement;
                output.Write(currentAsXElement.Value, formatDetails);
                handled = true;
            }
        }
        #endregion
    }

}
