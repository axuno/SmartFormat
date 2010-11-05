


 

namespace RTF
{
    using System;
    using System.Drawing;
    using System.Text;

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
                this._builder = builder;
                if (this._builder._unwrapped)
                {
                    return;
                }

                StringBuilder sb = this._builder._sb;

                int len = this._builder._sb.Length;

                if (this._builder._sf.Alignment == StringAlignment.Center)
                {
                    sb.Append("\\qc");
                }
                else if (this._builder._sf.Alignment == StringAlignment.Far)
                {
                    sb.Append("\\qr");
                }
                if ((this._builder._fontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
                {
                    sb.Append("\\b");
                }
                if ((this._builder._fontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
                {
                    sb.Append("\\i");
                }
                if ((this._builder._fontStyle & System.Drawing.FontStyle.Underline) == System.Drawing.FontStyle.Underline)
                {
                    sb.Append("\\ul");
                }
                if ((this._builder._fontStyle & System.Drawing.FontStyle.Strikeout) == System.Drawing.FontStyle.Strikeout)
                {
                    sb.Append("\\strike");
                }

                if (this._builder._fontSize != this._builder.DefaultFontSize)
                {
                    sb.AppendFormat("\\fs{0}", this._builder._fontSize);
                }
                if (this._builder._font != 0)
                {
                    sb.AppendFormat("\\f{0}", this._builder._font);
                }
                if (this._builder._forecolor != this._builder.Defaultforecolor)
                {
                    sb.AppendFormat("\\cf{0}", this._builder.IndexOf(this._builder._forecolor));
                }
                if (this._builder._backcolor != this._builder.DefaultBackColor)
                {
                    sb.AppendFormat("\\highlight{0}", this._builder.IndexOf(this._builder._backcolor));
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
                this.Dispose(false);
            }

            #endregion

            #region Methods

            protected void Dispose(bool disposing)
            {
                if (this._builder != null && !this._builder._unwrapped)
                {
        
                    StringBuilder sb = this._builder._sb;

                    int len = sb.Length;
                    if ((this._builder._fontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
                    {
                        sb.Append("\\b0");
                    }
                    if ((this._builder._fontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
                    {
                        sb.Append("\\i0");
                    }
                    if ((this._builder._fontStyle & System.Drawing.FontStyle.Underline) == System.Drawing.FontStyle.Underline)
                    {
                        sb.Append("\\ulnone");
                    }
                    if ((this._builder._fontStyle & System.Drawing.FontStyle.Strikeout) == System.Drawing.FontStyle.Strikeout)
                    {
                        sb.Append("\\strike0");
                    }

                    this._builder._fontStyle = System.Drawing.FontStyle.Regular;

                    if (this._builder._fontSize != this._builder.DefaultFontSize)
                    {
                        this._builder._fontSize = this._builder.DefaultFontSize;
                        sb.AppendFormat("\\fs{0} ", this._builder.DefaultFontSize);
                    }
                    if (this._builder._font != 0)
                    {
                        sb.Append("\\f0");
                        this._builder._font = 0;
                    }

                    if (this._builder._forecolor != this._builder.Defaultforecolor)
                    {
                        this._builder._forecolor = this._builder.Defaultforecolor;
                        sb.Append("\\cf0");
                    }
                    if (this._builder._backcolor != this._builder.DefaultBackColor)
                    {
                        this._builder._backcolor = this._builder.DefaultBackColor;
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
                this.Dispose(true);
            }

            #endregion
        }

        #endregion
    }
}


