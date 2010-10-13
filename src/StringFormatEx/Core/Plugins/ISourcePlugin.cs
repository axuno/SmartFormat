using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Plugins
{
    public interface ISourcePlugin
    {
        /// <summary>
        /// Takes the current object and evaluates the selector.
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="args"></param>
        /// <param name="current"></param>
        /// <param name="selector"></param>
        /// <param name="handled"></param>
        /// <param name="result"></param>
        void EvaluateSelector(SmartFormat formatter, object[] args, object current, string selector, ref bool handled, ref object result);
    }
}
