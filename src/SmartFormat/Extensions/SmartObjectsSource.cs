using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;

namespace SmartFormat.Extensions
{
    /// <summary>
    /// <see cref="SmartObjectsSource"/> processes <see cref="SmartObjects"/> as arguments to the formatter.
    /// </summary>
    [Obsolete("Depreciated in favor of ValueTupleSource", false)]
    public class SmartObjectsSource : ISource
    {
        private readonly SmartFormatter _formatter;

        /// <summary>
        /// Creates a new <see cref="SmartObjectsSource"/>.
        /// </summary>
        /// <param name="formatter"></param>
        [Obsolete("Depreciated in favor of ValueTupleSource", false)]
        public SmartObjectsSource(SmartFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Checks, if the current value of the <see cref="ISelectorInfo"/> is of type <see cref="SmartObjects"/>
        /// </summary>
        /// <param name="selectorInfo"></param>
        /// <returns>Returns true, if the current value of the <see cref="ISelectorInfo"/> is of type <see cref="SmartObjects"/>.</returns>
        [Obsolete("Depreciated in favor of ValueTupleSource", false)]
        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            if (!(selectorInfo is FormattingInfo formattingInfo)) return false;
            if (!(formattingInfo.CurrentValue is SmartObjects smartObjects)) return false;
            
            var savedCurrentValue = formattingInfo.CurrentValue;
            foreach (var obj in smartObjects)
            {
                foreach (var sourceExtension in _formatter.SourceExtensions)
                {
                    formattingInfo.CurrentValue = obj;
                    var handled = sourceExtension.TryEvaluateSelector(formattingInfo);
                    if (handled)
                    {
                        formattingInfo.CurrentValue = savedCurrentValue;
                        return true;
                    }
                }
            }

            formattingInfo.CurrentValue = savedCurrentValue;

            return false;
        }
    }
}