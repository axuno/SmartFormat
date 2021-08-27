


 

namespace RTF
{
    using System;
    using System.Drawing;

    public partial class RTFBuilder
    {
        #region Nested type: RTFParaWrap

        // ----------------------------------------------------------------------------------------
        //    _                ___        _..-._   Date: 12/11/08    23:36
        //    \`.|\..----...-'`   `-._.-'' _.-..'     
        //    /  ' `         ,       __.-'' 
        //    )/` _/     \   `-_,   /     Solution: RTFLib
        //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
        //        _.-'_./   {_.'   ; /    Author  : Anton
        //       {_.-``-'         {_/     Assembly: 1.0.0.0
        //                                Copyright © 2005-2008, Rogue Trader/MWM
        //        Project Item Name:      RTFBuilder.RTFParaWrap.cs - Code
        //        Purpose:                Wraps RtfBuilderbase injecting appropriate rtf codes after paragraph append 
        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Wraps RtfBuilderbase injecting appropriate rtf codes after paragraph append 
        /// </summary>
        private class RTFParaWrap : IDisposable
        {
            #region Fields

            private readonly RTFBuilder _builder;

            #endregion

            #region Constructor

            public RTFParaWrap(RTFBuilder builder)
            {
                this._builder = builder;
                int len = this._builder._sb.Length;
                if (this._builder._sf.Alignment == StringAlignment.Center)
                {
                    this._builder._sb.Append("\\qc");
                }
                else if (this._builder._sf.Alignment == StringAlignment.Far)
                {
                    this._builder._sb.Append("\\qr");
                }
                if (this._builder._firstLineIndent > 0)
                {
                    this._builder._sb.Append("\\fi" + this._builder._firstLineIndent);
                }
                if (this._builder._lineIndent > 0)
                {
                    this._builder._sb.Append("\\li" + this._builder._lineIndent);
                }


                if (this._builder._sb.Length > len)
                {
                    this._builder._sb.Append(" ");
                }
            }

            #endregion

            #region Override Methods

            ~RTFParaWrap()
            {
                this.Dispose(false);
            }

            #endregion

            #region Methods

            protected void Dispose(bool disposing)
            {
                if ( this._builder != null && !this._builder._unwrapped)
                {
                    if (this._builder._sf.Alignment != StringAlignment.Near || this._builder._lineIndent > 0 || this._builder._firstLineIndent > 0)
                    {
                        this._builder._firstLineIndent = 0;
                        this._builder._lineIndent = 0;
                        this._builder._sf.Alignment = StringAlignment.Near;
                        this._builder._sb.Append("\\pard ");
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


