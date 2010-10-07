using System;
using System.ComponentModel;



namespace StringFormatEx.Plugins.Core
{
    public enum CustomFormatPriorities
    {
        // the greater the value, the higher the priority
        /// <summary>
        /// This handler has the highest priority, so it gets fired first.
        /// </summary>
        Highest = 0,
        /// <summary>
        /// This handler has high priority, so it gets processed before normal priority ones.
        /// </summary>
        High = 1,
        /// <summary>
        /// [Default]
        /// This handler has normal priority.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Low priority is used only for the default built-in _GetDefaultSource/Output methods,
        /// and shouldn't be used otherwise.  
        /// If you want a handler to fire after the default methods, then use Lowest priority.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Low,

        /// <summary>
        /// This is the lowest priority.  It is only valid for CustomSource events, not CustomFormat events.
        /// A CustomSource event with this priority only occurs when reflection fails.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Lowest = 99
    }
}