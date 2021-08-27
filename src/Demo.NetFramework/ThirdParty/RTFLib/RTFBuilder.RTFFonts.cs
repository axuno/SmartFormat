namespace RTF
{
    using System;

    partial class RTFBuilder
    {
        #region Nested type: RawFonts


        // ----------------------------------------------------------------------------------------
        //    _                ___        _..-._   Date: 01/11/08    21:15
        //    \`.|\..----...-'`   `-._.-'' _.-..'     
        //    /  ' `         ,       __.-'' 
        //    )/` _/     \   `-_,   /     Solution: RTFLib
        //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
        //        _.-'_./   {_.'   ; /    Author  : Anton
        //       {_.-``-'         {_/     Assembly: 1.3.0.2499
        //                                Copyright © 2005-2008, Rogue Trader
        //        Project Item Name:      RTFBuilder.RTFFonts.cs - [PROJECT_ITEM_KIND]
        //        Purpose:                Used internally by RtfBuilderBase
        // ----------------------------------------------------------------------------------------


        internal static class RawFonts
        {
            #region Fields

            public const string Arial = @"{{\f{0}\fswiss\fprq2\fcharset0 Arial;}}";
            public const string ArialBlack = @"{{\f{0}\fswiss\fprq2\fcharset0 Arial Black;}}";
            public const string BookmanOldStyle = @"{{\f{0}\froman\fprq2\fcharset0 Bookman Old Style;}}";
            public const string Broadway = @"{{\f{0}\fdecor\fprq2\fcharset0 Broadway;}}";
            public const string CenturyGothic = @"{{\f{0}\fswiss\fprq2\fcharset0 Century Gothic;}}";
            public const string Consolas = @"{{\f{0}\fmodern\fprq1\fcharset0 Consolas;}}";
            public const string CordiaNew = @"{{\f{0}\fswiss\fprq2\fcharset0 Cordia New;}}";
            public const string CourierNew = @"{{\f{0}\fmodern\fprq1\fcharset0 Courier New;}}";
            public const string FontTimesNewRoman = @"{{\f{0}\froman\fcharset0 Times New Roman;}}";
            public const string Garamond = @"{{\f{0}\froman\fprq2\fcharset0 Garamond;}}";
            public const string Georgia = @"{{\f{0}\froman\fprq2\fcharset0 Georgia;}}";
            public const string Impact = @"{{\f{0}\fswiss\fprq2\fcharset0 Impact;}}";
            public const string LucidaConsole = @"{{\f{0}\fmodern\fprq1\fcharset0 Lucida Console;}}";
            public const string MSSansSerif = "{{\f{0}\fswiss\fprq2\fcharset0 MS Reference Sans Serif;}}";
            public const string Symbol = @"{{\f{0}\ftech\fcharset0 Symbol;}}";
            public const string WingDings = "{{\f{0}\fnil\fprq2\fcharset2 Wingdings;}}";

            #endregion

            #region Static Methods

            public static string GetKnownFontstring(RTFFont font)
            {
                string value = string.Empty;
                switch (font)
                {
                    case RTFFont.Arial:
                        value = Arial;
                        break;
                    case RTFFont.ArialBlack:
                        value = ArialBlack;
                        break;
                    case RTFFont.BookmanOldStyle:
                        value = BookmanOldStyle;
                        break;
                    case RTFFont.Broadway:
                        value = Broadway;
                        break;
                    case RTFFont.CenturyGothic:
                        value = CenturyGothic;
                        break;
                    case RTFFont.Consolas:
                        value = Consolas;
                        break;
                    case RTFFont.CordiaNew:
                        value = CordiaNew;
                        break;
                    case RTFFont.CourierNew:
                        value = CourierNew;
                        break;
                    case RTFFont.FontTimesNewRoman:
                        value = FontTimesNewRoman;
                        break;
                    case RTFFont.Garamond:
                        value = Garamond;
                        break;
                    case RTFFont.Georgia:
                        value = Georgia;
                        break;
                    case RTFFont.Impact:
                        value = Impact;
                        break;
                    case RTFFont.LucidaConsole:
                        value = LucidaConsole;
                        break;
                    case RTFFont.Symbol:
                        value = Symbol;
                        break;
                    case RTFFont.WingDings:
                        value = WingDings;
                        break;
                    case RTFFont.MSSansSerif:
                        value = MSSansSerif;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("font");
                }
                return value;
            }

            #endregion
        }

        #endregion
    }
}