#nullable disable

using System;
using System.Drawing;

namespace Demo.ThirdParty.RTFLib;

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
            _builder = builder;
            int len = _builder._sb.Length;
            if (_builder._sf.Alignment == StringAlignment.Center)
            {
                _builder._sb.Append("\\qc");
            }
            else if (_builder._sf.Alignment == StringAlignment.Far)
            {
                _builder._sb.Append("\\qr");
            }
            if (_builder._firstLineIndent > 0)
            {
                _builder._sb.Append("\\fi" + _builder._firstLineIndent);
            }
            if (_builder._lineIndent > 0)
            {
                _builder._sb.Append("\\li" + _builder._lineIndent);
            }


            if (_builder._sb.Length > len)
            {
                _builder._sb.Append(" ");
            }
        }

        #endregion

        #region Override Methods

        ~RTFParaWrap()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        protected void Dispose(bool disposing)
        {
            if ( _builder != null && !_builder._unwrapped)
            {
                if (_builder._sf.Alignment != StringAlignment.Near || _builder._lineIndent > 0 || _builder._firstLineIndent > 0)
                {
                    _builder._firstLineIndent = 0;
                    _builder._lineIndent = 0;
                    _builder._sf.Alignment = StringAlignment.Near;
                    _builder._sb.Append("\\pard ");
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
