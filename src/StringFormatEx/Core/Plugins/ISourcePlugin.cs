using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringFormatEx.Core.Plugins
{
    public interface ISourcePlugin
    {
        void EvaluateSelector(object arg, string selector, ref bool handled, ref object result);
    }
    public delegate void EvaluateSelectorDelegate(object arg, string selector, ref bool handled, ref object result);

}
