using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Output;
using StringFormatEx.Plugins;

namespace StringFormatEx.Core
{
    /// <summary>
    /// This class holds a Default instance of the SmartFormat.
    /// The default instance has all plugins registered.
    /// </summary>
    public static class Smart
    {
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

        #region: Smart.Format :

        public static string Format(string format, params object[] args)
        {
            return Default.Format(format, args);
        }

        #endregion

        #region: Default formatter :

        private static SmartFormat _default;
        public static SmartFormat Default
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

        public static SmartFormat CreateDefaultSmartFormat()
        {
            // Register all default plugins here:
            var result = new SmartFormat();
            result.AddSourcePlugins(
                //new ArrayPlugin(),
                //new ReflectionPlugin()
                );
            result.AddFormatterPlugins(
                //new ConditionalPlugin(),
                //new ArrayPlugin(),
                //new TimestringPlugin()
                );

            return result;
        }

        #endregion
    }
}
