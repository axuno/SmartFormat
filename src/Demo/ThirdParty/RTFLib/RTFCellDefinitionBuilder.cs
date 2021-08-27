


 

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
                this._builder = builder;

                this._definitionBuilder = definitionBuilder;
                this._cellDefinition = cellDefinition;


                this.AppendDefinition();
            }

            #endregion

            #region Public Properties

            public RTFCellDefinition CellDefinition
            {
                get { return this._cellDefinition; }
            }

            #endregion

            #region Methods

            private void AppendDefinition()
            {
                this.CellAlignment();
                this.TableCellBorderSide();
                //Pad();


                this._definitionBuilder.AppendFormat("\\cellx{0}", (int) (this._cellDefinition.CellWidthRaw * TWIPSA4) + this._cellDefinition.X);
                //_definitionBuilder.AppendFormat("\\clwWidth{0}", _cellDefinition.CellWidth);

                //_definitionBuilder.Append("\\cltxlrtb\\clFitText");

                this._definitionBuilder.AppendLine();


                //Cell text flow
            }

            private string BorderDef()
            {
                StringBuilder sb = new StringBuilder();
                RTFBorderSide _rTFBorderSide = this._cellDefinition.RTFBorderSide;
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
                sb.Append(this._cellDefinition.BorderWidth);

                sb.Append("\\brdrcf");
                sb.Append(this._builder.IndexOf(this._cellDefinition.BorderColor));

                return sb.ToString();
            }

            private void CellAlignment()
            {
                switch (this._cellDefinition.Alignment)
                {
                    case RTFAlignment.BottomCenter:
                    case RTFAlignment.BottomLeft:
                    case RTFAlignment.BottomRight:
                        this._definitionBuilder.Append("\\clvertalb"); //\\qr
                        break;
                    case RTFAlignment.MiddleCenter:
                    case RTFAlignment.MiddleLeft:
                    case RTFAlignment.MiddleRight:
                        this._definitionBuilder.Append("\\clvertalc"); //\\qr
                        break;
                    case RTFAlignment.TopCenter:
                    case RTFAlignment.TopLeft:
                    case RTFAlignment.TopRight:
                        this._definitionBuilder.Append("\\clvertalt"); //\\qr
                        break;
                }
            }

            private void Pad()
            {
                if (this._cellDefinition.Padding != Padding.Empty)
                {
                    StringBuilder sb = this._definitionBuilder;
                    sb.AppendFormat("\\clpadfl3\\clpadl{0}", this._cellDefinition.Padding.Left);
                    sb.AppendFormat("\\clpadlr3\\clpadr{0}", this._cellDefinition.Padding.Right);
                    sb.AppendFormat("\\clpadlt3\\clpadt{0}", this._cellDefinition.Padding.Top);
                    sb.AppendFormat("\\clpadlb3\\clpadb{0}", this._cellDefinition.Padding.Bottom);
                }
            }

            private void TableCellBorderSide()
            {
                RTFBorderSide _rTFBorderSide = this._cellDefinition.RTFBorderSide;

                if (_rTFBorderSide != RTFBorderSide.None)
                {
                    StringBuilder sb = this._definitionBuilder;
                    string bd = this.BorderDef();
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


