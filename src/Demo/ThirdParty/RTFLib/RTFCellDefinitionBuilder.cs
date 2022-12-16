


 

namespace RTF
{
    using System.Text;
    using System.Windows.Forms;

    public partial class RTFBuilder
    {
        #region Nested type: RTFCellDefinitionBuilder

        // ----------------------------------------------------------------------------------------
        //    _                ___        _..-._   Date: 12/11/08    23:47
        //    \`.|\..----...-'`   `-._.-'' _.-..'     
        //    /  ' `         ,       __.-'' 
        //    )/` _/     \   `-_,   /     Solution: RTFLib
        //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
        //        _.-'_./   {_.'   ; /    Author  : Anton
        //       {_.-``-'         {_/     Assembly: 1.0.0.0
        //                                Copyright © 2005-2008, Rogue Trader/MWM
        //        Project Item Name:      RTFCellDefinitionBuilder.cs - Code
        //        Purpose:                Injects Cell Rtf Codes
        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Injects Cell Rtf Codes
        /// </summary>
        private class RTFCellDefinitionBuilder
        {
            #region Fields

            private readonly RTFBuilder _builder;
            private readonly StringBuilder _definitionBuilder;
            private RTFCellDefinition _cellDefinition;

            #endregion

            #region Constructor

            internal RTFCellDefinitionBuilder(RTFBuilder builder, StringBuilder definitionBuilder, RTFCellDefinition cellDefinition)
            {
                _builder = builder;

                _definitionBuilder = definitionBuilder;
                _cellDefinition = cellDefinition;


                AppendDefinition();
            }

            #endregion

            #region Public Properties

            public RTFCellDefinition CellDefinition
            {
                get { return _cellDefinition; }
            }

            #endregion

            #region Methods

            private void AppendDefinition()
            {
                CellAlignment();
                TableCellBorderSide();
                //Pad();


                _definitionBuilder.AppendFormat("\\cellx{0}", (int) (_cellDefinition.CellWidthRaw * TWIPSA4) + _cellDefinition.X);
                //_definitionBuilder.AppendFormat("\\clwWidth{0}", _cellDefinition.CellWidth);

                //_definitionBuilder.Append("\\cltxlrtb\\clFitText");

                _definitionBuilder.AppendLine();


                //Cell text flow
            }

            private string BorderDef()
            {
                StringBuilder sb = new StringBuilder();
                RTFBorderSide _rTFBorderSide = _cellDefinition.RTFBorderSide;
                if ((_rTFBorderSide & RTFBorderSide.DoubleThickness) == RTFBorderSide.DoubleThickness)
                {
                    sb.Append("\\brdrth");
                }
                else
                {
                    sb.Append("\\brdrs");
                }
                if ((_rTFBorderSide & RTFBorderSide.DoubleBorder) == RTFBorderSide.DoubleBorder)
                {
                    sb.Append("\\brdrdb");
                }
                sb.Append("\\brdrw");
                sb.Append(_cellDefinition.BorderWidth);

                sb.Append("\\brdrcf");
                sb.Append(_builder.IndexOf(_cellDefinition.BorderColor));

                return sb.ToString();
            }

            private void CellAlignment()
            {
                switch (_cellDefinition.Alignment)
                {
                    case RTFAlignment.BottomCenter:
                    case RTFAlignment.BottomLeft:
                    case RTFAlignment.BottomRight:
                        _definitionBuilder.Append("\\clvertalb"); //\\qr
                        break;
                    case RTFAlignment.MiddleCenter:
                    case RTFAlignment.MiddleLeft:
                    case RTFAlignment.MiddleRight:
                        _definitionBuilder.Append("\\clvertalc"); //\\qr
                        break;
                    case RTFAlignment.TopCenter:
                    case RTFAlignment.TopLeft:
                    case RTFAlignment.TopRight:
                        _definitionBuilder.Append("\\clvertalt"); //\\qr
                        break;
                }
            }

            private void Pad()
            {
                if (_cellDefinition.Padding != Padding.Empty)
                {
                    StringBuilder sb = _definitionBuilder;
                    sb.AppendFormat("\\clpadfl3\\clpadl{0}", _cellDefinition.Padding.Left);
                    sb.AppendFormat("\\clpadlr3\\clpadr{0}", _cellDefinition.Padding.Right);
                    sb.AppendFormat("\\clpadlt3\\clpadt{0}", _cellDefinition.Padding.Top);
                    sb.AppendFormat("\\clpadlb3\\clpadb{0}", _cellDefinition.Padding.Bottom);
                }
            }

            private void TableCellBorderSide()
            {
                RTFBorderSide _rTFBorderSide = _cellDefinition.RTFBorderSide;

                if (_rTFBorderSide != RTFBorderSide.None)
                {
                    StringBuilder sb = _definitionBuilder;
                    string bd = BorderDef();
                    if (_rTFBorderSide == RTFBorderSide.None)
                    {
                        sb.Append("\\brdrnil");
                    }
                    else
                    {
                        if ((_rTFBorderSide & RTFBorderSide.Left) == RTFBorderSide.Left)
                        {
                            sb.Append("\\clbrdrl").Append(bd);
                        }
                        if ((_rTFBorderSide & RTFBorderSide.Right) == RTFBorderSide.Right)
                        {
                            sb.Append("\\clbrdrr").Append(bd);
                        }
                        if ((_rTFBorderSide & RTFBorderSide.Top) == RTFBorderSide.Top)
                        {
                            sb.Append("\\clbrdrt").Append(bd);
                        }
                        if ((_rTFBorderSide & RTFBorderSide.Bottom) == RTFBorderSide.Bottom)
                        {
                            sb.Append("\\clbrdrb").Append(bd);
                        }
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}


