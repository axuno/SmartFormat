using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Plugins
{
    public class DefaultSource : ISourcePlugin
    {
        /// <summary>
        /// Performs the default index-based selector, same as String.Format.
        /// </summary>
        public void EvaluateSelector(SmartFormatter formatter, object[] args, object current, Selector selector, ref bool handled, ref object result)
        {
            // Make sure the selector is a valid in-range index:
            int argIndex;
            if (int.TryParse(selector.Text, out argIndex) && argIndex < args.Length)
            {
                result = args[argIndex];
                handled = true;
            }
        }
    }
}
