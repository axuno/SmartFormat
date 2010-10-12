using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StringFormatEx.Core.Output;

namespace StringFormatEx.Core
{
    public static class Extensions
    {
        #region: StringBuilder :

        public static void AppendSmart(this StringBuilder sb, string format, params object[] args)
        {
            var so = new StringOutput(sb);
            Smart.Default.FormatInto(so, format, args);
        }

        #endregion

        #region: String :

        public static string FormatSmart(this string format, params object[] args)
        {
            return Smart.Format(format, args);
        }

        #endregion
    }
}
