


 

namespace RTF
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// 
    /// </summary>
    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:49
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      RTFRowDefinition.cs - Code
    //        Purpose:                Definition of Rich Table Row
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Definition of Rich Table Row
    /// </summary>
    public struct RTFRowDefinition
    {
        #region Fields

        private readonly RTFAlignment _alignment;
        private readonly Color _borderColor;
        private readonly int _borderWidth;
        private readonly Padding _padding;
        private readonly RTFBorderSide _rTFBorderSide;
        private int _rowWidth;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RTFRowDefinition"/> struct.
        /// </summary>
        /// <param name="rowWidth">Width of the row.</param>
        /// <param name="alignment">The alignment.</param>
        /// <param name="rTFBorderSide">The RTFBorderSide.</param>
        /// <param name="borderWidth">Width of the border.</param>
        /// <param name="borderColor">Color of the border.</param>
        public RTFRowDefinition(int rowWidth, RTFAlignment alignment, RTFBorderSide rTFBorderSide, int borderWidth, Color borderColor, Padding padding)
        {
            this._padding = padding;
            this._alignment = alignment;
            this._rTFBorderSide = rTFBorderSide;
            this._borderWidth = borderWidth;
            this._borderColor = borderColor;
            this._rowWidth = rowWidth * RTFBuilder.TWIPSA4 / 100;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the alignment.
        /// </summary>
        /// <value>The alignment.</value>
        public RTFAlignment Alignment
        {
            [DebuggerStepThrough]
            get { return this._alignment; }
        }

        public Padding Padding
        {
            [DebuggerStepThrough]
            get { return this._padding; }
        }

        /// <summary>
        /// Gets the RTF border side.
        /// </summary>
        /// <value>The RTF border side.</value>
        public RTFBorderSide RTFBorderSide
        {
            [DebuggerStepThrough]
            get { return this._rTFBorderSide; }
        }

        /// <summary>
        /// Gets the width of the border.
        /// </summary>
        /// <value>The width of the border.</value>
        public int BorderWidth
        {
            [DebuggerStepThrough]
            get { return this._borderWidth; }
        }

        /// <summary>
        /// Gets the color of the border.
        /// </summary>
        /// <value>The color of the border.</value>
        public Color BorderColor
        {
            [DebuggerStepThrough]
            get { return this._borderColor; }
        }

        /// <summary>
        /// Gets or sets the width of the cell.
        /// </summary>
        /// <value>The width of the cell.</value>
        public int RowWidth
        {
            [DebuggerStepThrough]
            get { return this._rowWidth; }
            [DebuggerStepThrough]
            set { this._rowWidth = value; }
        }

        #endregion
    }
}


