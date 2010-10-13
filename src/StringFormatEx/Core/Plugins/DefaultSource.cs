using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Plugins
{
    public class DefaultSource : ISourcePlugin
    {
        /// <summary>
        /// Performs the default index-based selector
        /// </summary>
        public void EvaluateSelector(SmartFormat formatter, object[] args, object current, string selector, ref bool handled, ref object result)
        {
            // Make sure the selector is a valid in-range index:
            int argIndex;
            if (int.TryParse(selector, out argIndex) && argIndex < args.Length)
            {
                result = args[argIndex];
                handled = true;
            }
        }
    }
}
