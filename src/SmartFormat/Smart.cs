using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartFormat.Core.Output;
using SmartFormat.Plugins;
using SmartFormat.Core;
using SmartFormat.Core.Plugins;

namespace SmartFormat
{
    /// <summary>
    /// This class holds a Default instance of the SmartFormatter.
    /// The default instance has all plugins registered.
    /// </summary>
    public static class Smart
    {
        #region: Smart.Format :

        public static string Format(string format, params object[] args)
        {
            return Default.Format(format, args);
        }

        #endregion

        #region: Overloads - Just to match the signature of String.Format :

        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            return Format(format, new object[] { arg0, arg1, arg2 });
        }
        public static string Format(string format, object arg0, object arg1)
        {
            return Format(format, new object[] { arg0, arg1 });
        }
        public static string Format(string format, object arg0)
        {
            return Format(format, new object[] { arg0 });
        }

        #endregion

        #region: Default formatter :

        private static SmartFormatter _default;
        public static SmartFormatter Default
        {
            get
            {
                if (_default == null)
                    _default = CreateDefaultSmartFormat();
                return _default;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _default = value;
            }
        }

        public static SmartFormatter CreateDefaultSmartFormat()
        {
            // Register all default plugins here:
            var result = new SmartFormatter();
            result.AddPlugins(
                // Add all plugins:
                new ArrayPlugin(result),
                new ConditionalPlugin(),
                new ReflectionPlugin(result),
                new TimestringPlugin(),
                new DefaultSource(),
                new DefaultFormatter()
                );

            return result;
        }

        #endregion
    }
}
