


namespace RTF
{
    using System;

    public partial class RTFBuilder
    {
        #region Nested type: RTFCell

        // ----------------------------------------------------------------------------------------
        //    _                ___        _..-._   Date: 12/11/08    23:46
        //    \`.|\..----...-'`   `-._.-'' _.-..'     
        //    /  ' `         ,       __.-'' 
        //    )/` _/     \   `-_,   /     Solution: RTFLib
        //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
        //        _.-'_./   {_.'   ; /    Author  : Anton
        //       {_.-``-'         {_/     Assembly: 1.0.0.0
        //                                Copyright © 2005-2008, Rogue Trader/MWM
        //        Project Item Name:      RTFCell.cs - Code
        //        Purpose:                Cell In Table Row
        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Cell In Table Row
        /// </summary>
        public class RTFCell : IBuilderContent
        {
            #region Fields

            private RTFBuilder _builder;
            private RTFCellDefinition _cellDefinition;
            private bool _firstAccessContent;

            #endregion

            #region Constructor

            public RTFCell(RTFBuilder builder, RTFCellDefinition cellDefinition)
            {
                _builder = builder;
                _cellDefinition = cellDefinition;
                _firstAccessContent = true;
            }

            #endregion

            #region Override Methods

            ~RTFCell()
            {
                Dispose(false);
            }

            #endregion

            #region Methods

            protected void Dispose(bool disposing)
            {
                if (disposing && _builder != null)
                {
                    _builder._sb.AppendLine("\\cell ");
                }
                _builder = null;
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }

            #endregion

            #region IBuilderContent Members

            public void Dispose()
            {
                Dispose(true);
            }

            public RTFBuilderbase Content
            {
                get
                {
                    if (_firstAccessContent)
                    {
                        //par in table
                        switch (_cellDefinition.Alignment)
                        {
                            case RTFAlignment.TopCenter:
                            case RTFAlignment.BottomCenter:
                            case RTFAlignment.MiddleCenter:
                                _builder._sb.Append("\\qc ");
                                break;
                            case RTFAlignment.TopLeft:
                            case RTFAlignment.MiddleLeft:
                            case RTFAlignment.BottomLeft:
                                _builder._sb.Append("\\ql ");
                                break;
                            case RTFAlignment.TopRight:
                            case RTFAlignment.BottomRight:
                            case RTFAlignment.MiddleRight:
                                _builder._sb.Append("\\qr ");
                                break;
                        }
                        _firstAccessContent = false;
                    }
                    return _builder;
                }
            }

            #endregion
        }

        #endregion
    }
}
