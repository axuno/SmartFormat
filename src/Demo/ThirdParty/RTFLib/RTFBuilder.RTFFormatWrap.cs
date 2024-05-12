#nullable disable

using System;
using System.Drawing;
using System.Text;

namespace Demo.ThirdParty.RTFLib;

partial class RTFBuilder
{
    #region Nested type: RTFFormatWrap

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:35
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      RTFBuilder.RTFFormatWrap.cs - Code
    //        Purpose:                Wraps RTFBuilderbase for formatting changes allowing injection of appropriate rtf codes to revert format after each Append (string) call
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Wraps RTFBuilderbase for formatting changes allowing injection of appropriate rtf codes to revert format after each Append (string) call
    /// </summary>
    private class RTFFormatWrap : IDisposable
    {
        #region Fields

        private readonly RTFBuilder _builder;

        #endregion

        #region Constructor

        public RTFFormatWrap(RTFBuilder builder)
        {
            _builder = builder;
            if (_builder._unwrapped)
            {
                return;
            }

            StringBuilder sb = _builder._sb;

            int len = _builder._sb.Length;

            if (_builder._sf.Alignment == StringAlignment.Center)
            {
                sb.Append("\\qc");
            }
            else if (_builder._sf.Alignment == StringAlignment.Far)
            {
                sb.Append("\\qr");
            }
            if ((_builder._fontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
            {
                sb.Append("\\b");
            }
            if ((_builder._fontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
            {
                sb.Append("\\i");
            }
            if ((_builder._fontStyle & System.Drawing.FontStyle.Underline) == System.Drawing.FontStyle.Underline)
            {
                sb.Append("\\ul");
            }
            if ((_builder._fontStyle & System.Drawing.FontStyle.Strikeout) == System.Drawing.FontStyle.Strikeout)
            {
                sb.Append("\\strike");
            }

            if (_builder._fontSize != _builder.DefaultFontSize)
            {
                sb.AppendFormat("\\fs{0}", _builder._fontSize);
            }
            if (_builder._font != 0)
            {
                sb.AppendFormat("\\f{0}", _builder._font);
            }
            if (_builder._forecolor != _builder.Defaultforecolor)
            {
                sb.AppendFormat("\\cf{0}", _builder.IndexOf(_builder._forecolor));
            }
            if (_builder._backcolor != _builder.DefaultBackColor)
            {
                sb.AppendFormat("\\highlight{0}", _builder.IndexOf(_builder._backcolor));
            }


            if (sb.Length > len)
            {
                sb.Append(" ");
            }
        }

        #endregion

        #region Override Methods

        ~RTFFormatWrap()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        protected void Dispose(bool disposing)
        {
            if (_builder != null && !_builder._unwrapped)
            {
        
                StringBuilder sb = _builder._sb;

                int len = sb.Length;
                if ((_builder._fontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
                {
                    sb.Append("\\b0");
                }
                if ((_builder._fontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
                {
                    sb.Append("\\i0");
                }
                if ((_builder._fontStyle & System.Drawing.FontStyle.Underline) == System.Drawing.FontStyle.Underline)
                {
                    sb.Append("\\ulnone");
                }
                if ((_builder._fontStyle & System.Drawing.FontStyle.Strikeout) == System.Drawing.FontStyle.Strikeout)
                {
                    sb.Append("\\strike0");
                }

                _builder._fontStyle = System.Drawing.FontStyle.Regular;

                if (_builder._fontSize != _builder.DefaultFontSize)
                {
                    _builder._fontSize = _builder.DefaultFontSize;
                    sb.AppendFormat("\\fs{0} ", _builder.DefaultFontSize);
                }
                if (_builder._font != 0)
                {
                    sb.Append("\\f0");
                    _builder._font = 0;
                }

                if (_builder._forecolor != _builder.Defaultforecolor)
                {
                    _builder._forecolor = _builder.Defaultforecolor;
                    sb.Append("\\cf0");
                }
                if (_builder._backcolor != _builder.DefaultBackColor)
                {
                    _builder._backcolor = _builder.DefaultBackColor;
                    sb.Append("\\highlight0");
                }
                //if (_builder._alignment != StringAlignment.Near )
                //{
                //    _builder._alignment = StringAlignment.Near;
                //    sb.Append("\\ql");
                //}
                if (sb.Length > len)
                {
                    sb.Append(" ");
                }
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    #endregion
}
