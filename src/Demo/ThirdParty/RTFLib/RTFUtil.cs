


 

namespace RTF
{
    using System.Text;

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:50
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      RTFUtil.cs - Code
    //        Purpose:                A Work in Progress
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// A Work in Progress
    /// </summary>
    public class RTFUtil
    {
        #region Public Methods

        public void ParagraphBorderSide(StringBuilder sb, RTFBorderSide rTFBorderSide)
        {
            if (rTFBorderSide == RTFBorderSide.None)
            {
                return;
            }
            if ((rTFBorderSide & RTFBorderSide.Left) == RTFBorderSide.Left)
            {
                sb.Append("\\brdrl");
            }
            if ((rTFBorderSide & RTFBorderSide.Right) == RTFBorderSide.Right)
            {
                sb.Append("\\brdrr");
            }
            if ((rTFBorderSide & RTFBorderSide.Top) == RTFBorderSide.Top)
            {
                sb.Append("\\brdrt");
            }
            if ((rTFBorderSide & RTFBorderSide.Bottom) == RTFBorderSide.Bottom)
            {
                sb.Append("\\brdrb");
            }

            if ((rTFBorderSide & RTFBorderSide.DoubleThickness) == RTFBorderSide.DoubleThickness)
            {
                sb.Append("\\brdrth");
            }
            else
            {
                sb.Append("\\brdrs");
            }
            if ((rTFBorderSide & RTFBorderSide.DoubleBorder) == RTFBorderSide.DoubleBorder)
            {
                sb.Append("\\brdrdb");
            }
            sb.Append("\\brdrw10");
        }

        public void TableRowBorderSide(StringBuilder sb, RTFBorderSide rTFBorderSide)
        {
            if (rTFBorderSide == RTFBorderSide.None)
            {
                return;
            }
            if ((rTFBorderSide & RTFBorderSide.Left) == RTFBorderSide.Left)
            {
                sb.Append("\\trbrdrl");
            }
            if ((rTFBorderSide & RTFBorderSide.Right) == RTFBorderSide.Right)
            {
                sb.Append("\\trbrdrr");
            }
            if ((rTFBorderSide & RTFBorderSide.Top) == RTFBorderSide.Top)
            {
                sb.Append("\\trbrdrt");
            }
            if ((rTFBorderSide & RTFBorderSide.Bottom) == RTFBorderSide.Bottom)
            {
                sb.Append("\\trbrdrb");
            }
            if ((rTFBorderSide & RTFBorderSide.DoubleThickness) == RTFBorderSide.DoubleThickness)
            {
                sb.Append("\\brdrth");
            }
            else
            {
                sb.Append("\\brdrs");
            }
            if ((rTFBorderSide & RTFBorderSide.DoubleBorder) == RTFBorderSide.DoubleBorder)
            {
                sb.Append("\\brdrdb");
            }
            sb.Append("\\brdrw10");
        }

        #endregion
    }
}


