using System;
using System.Collections.Generic;



namespace StringFormatEx.Plugins.Core
{
    public interface IStringFormatterPlugin
    {
        IEnumerable<EventHandler<ExtendSourceEventArgs>> GetSourceExtensions();
        IEnumerable<EventHandler<ExtendFormatEventArgs>> GetFormatExtensions();
    }
}