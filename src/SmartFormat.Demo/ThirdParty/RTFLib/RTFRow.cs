



 

namespace RTF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;

    public partial class RTFBuilder
    {
        #region Fields
        /// <summary>
        /// 
        /// </summary>
        internal const int TWIPSA4 = 11907;
        /// <summary>
        /// 
        /// </summary>
        internal const int TWIPSA4V = 16840;

        #endregion

        #region Nested type: RTFRow


        // ----------------------------------------------------------------------------------------
        //    _                ___        _..-._   Date: 12/11/08    23:49
        //    \`.|\..----...-'`   `-._.-'' _.-..'     
        //    /  ' `         ,       __.-'' 
        //    )/` _/     \   `-_,   /     Solution: RTFLib
        //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
        //        _.-'_./   {_.'   ; /    Author  : Anton
        //       {_.-``-'         {_/     Assembly: 1.0.0.0
        //                                Copyright © 2005-2008, Rogue Trader/MWM
        //        Project Item Name:      RTFRow.cs - Code
        //        Purpose:                Rich Table Row
        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Rich Table Row
        /// </summary>
        private class RTFRow : IRTFRow
        {


            #region Fields

            private readonly RTFCellDefinition[] _cellDefinitions;
            private readonly List <RTFCellDefinitionBuilder> _cells;
            private readonly StringBuilder _definitionBuilder;
            private RTFBuilder _builder;
            private RTFRowDefinition _rowDefinition;

            #endregion

            #region Constructor

            internal RTFRow(RTFBuilder builder, RTFRowDefinition rowDefinition, RTFCellDefinition[] cellDefinitions)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException("builder");
                }
                if (cellDefinitions == null)
                {
                    throw new ArgumentNullException("cellDefinitions");
                }
                if (rowDefinition.RowWidth == 0)
                {
                    throw new ArgumentNullException("rowDefinition.RowWidth");
                }
                if (cellDefinitions.Length == 0)
                {
                    throw new ArgumentNullException("cellDefinitions.Length");
                }
                this._definitionBuilder = new StringBuilder();


                this._rowDefinition = rowDefinition;
                this._cellDefinitions = cellDefinitions;
                this._builder = builder;

                StringBuilder sb = this._definitionBuilder;

                sb.Append("\\trowd\\trgaph115\\trleft-115");
                sb.AppendLine("\\trautofit1"); //AutoFit: ON
                this.TableCellBorderSide();
                this.BorderDef();
                //Pad();


                // \trhdr    Table row header. This row should appear at the top of every page on which the current table appears.
                // \trkeep    Keep table row together. This row cannot be split by a page break. This property is assumed to be off unless the control word is present.
                //\trleftN    Position in twips of the leftmost edge of the table with respect to the left edge of its column.
                //\trqc    Centers a table row with respect to its containing column.
                //\trql    Left-justifies a table row with respect to its containing column.
                //\trqr    Right-justifies a table row with respect to its containing column.
                //\trrhN    Height of a table row in twips. When 0, the height is sufficient for all the text in the line; when positive, the height is guaranteed to be at least the specified height; when negative, the absolute value of the height is used, regardless of the height of the text in the line.
                //\trpaddbN    Default bottom cell margin or padding for the row.
                //\trpaddlN    Default left cell margin or padding for the row.
                //\trpaddrN    Default right cell margin or padding for the row.
                //\trpaddtN    Default top cell margin or padding for the row.
                //\trpaddfbN    Units for \trpaddbN:
                //0    Null. Ignore \trpaddbN in favor of \trgaphN (Word 97 style padding).
                //3    Twips.
                //\trpaddflN    Units for \trpaddlN:
                //0    Null. Ignore \trpaddlN in favor of \trgaphN (Word 97 style padding).
                //3    Twips.
                //\trpaddfrN    Units for \trpaddrN:
                //0    Null. Ignore \trpaddrN in favor of \trgaphN (Word 97 style padding).
                //3    Twips.
                //\trpaddftN    Units for \trpaddtN:
                //0    Null. Ignore \trpaddtN in favor of \trgaphN (Word 97 style padding).
                //3    Twips.


                this._cells = new List <RTFCellDefinitionBuilder>();
                int x = 0;
                foreach (RTFCellDefinition item in this._cellDefinitions)
                {
                    item.SetX(x);
                    x += (int) (item.CellWidthRaw * TWIPSA4);
                    this._cells.Add(new RTFCellDefinitionBuilder(this._builder, this._definitionBuilder, item));
                }
                this._builder._sb.Append(this._definitionBuilder.ToString());
            }

            #endregion

            #region Override Methods

            ~RTFRow()
            {
                this.Dispose(false);
            }

            #endregion

            #region Methods

            private string BorderDef()
            {
                StringBuilder sb = new StringBuilder();
                RTFBorderSide _rTFBorderSide = this._rowDefinition.RTFBorderSide;
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
                sb.Append(this._rowDefinition.BorderWidth);

                sb.AppendFormat("\\brdrc{0}", this._builder.IndexOf(this._rowDefinition.BorderColor));
                sb.AppendLine();

                return sb.ToString();
            }

            protected void Dispose(bool disposing)
            {
                if (this._builder != null)
                {
                    this._builder._sb.AppendLine("\\row");
                    //_builder._sb.AppendLine();
                    this._builder._sb.AppendLine("{");
                    this._builder._sb.Append(this._definitionBuilder.ToString());

                    this._builder._sb.AppendLine("}");
                }
                this._builder = null;
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }

            private void Pad()
            {
                if (this._rowDefinition.Padding != Padding.Empty)
                {
                    StringBuilder sb = this._definitionBuilder;
                    sb.AppendFormat("\\trpaddfl3\\trpaddl{0}", this._rowDefinition.Padding.Left);
                    sb.AppendFormat("\\trpaddfr3\\trpaddr{0}", this._rowDefinition.Padding.Right);
                    sb.AppendFormat("\\trpaddft3\\trpaddt{0}", this._rowDefinition.Padding.Top);
                    sb.AppendFormat("\\trpaddfb3\\trpaddb{0}", this._rowDefinition.Padding.Bottom);
                }
            }

            private void TableCellBorderSide()
            {
                RTFBorderSide _rTFBorderSide = this._rowDefinition.RTFBorderSide;

                if (_rTFBorderSide != RTFBorderSide.None)
                {
                    StringBuilder sb = this._definitionBuilder;
                    string bd = this.BorderDef();

                    if ((_rTFBorderSide & RTFBorderSide.Left) == RTFBorderSide.Left)
                    {
                        sb.Append("\\trbrdrl").Append(bd);
                    }
                    if ((_rTFBorderSide & RTFBorderSide.Right) == RTFBorderSide.Right)
                    {
                        sb.Append("\\trbrdrr").Append(bd);
                    }
                    if ((_rTFBorderSide & RTFBorderSide.Top) == RTFBorderSide.Top)
                    {
                        sb.Append("\\trbrdrt").Append(bd);
                    }
                    if ((_rTFBorderSide & RTFBorderSide.Bottom) == RTFBorderSide.Bottom)
                    {
                        sb.Append("\\trbrdrb").Append(bd);
                    }
                    //sb.Append("\\trbrdrh\\brdrs\\brdrw10");
                    //sb.Append("\\trbrdrv\\brdrs\\brdrw10");

                    sb.AppendLine();
                }
            }

            #endregion

            #region IRTFRow Members

            public IEnumerator <IBuilderContent> GetEnumerator()
            {
                this._builder._sb.AppendLine("\\pard\\intbl\\f0");
                foreach (RTFCellDefinitionBuilder item in this._cells)
                {
                    yield return new RTFCell(this._builder, item.CellDefinition);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public void Dispose()
            {
                this.Dispose(true);
            }

            #endregion
        }

        #endregion
    }
}


